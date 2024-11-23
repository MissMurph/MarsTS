using System;
using System.Collections.Generic;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Buildings
{
    public class BuildingConstructionGhost : NetworkBehaviour,
        ISelectable,
        ITaggable<BuildingConstructionGhost>,
        IAttackable,
        ICommandable
    {
        public GameObject GameObject => gameObject;
        public IUnit Unit => this;

        /*  ISelectable Properties  */
        public int Id => _entityComponent.Id;
        public string UnitType { get; private set; }

        public string RegistryKey => "buildingConstructionGhost:" + UnitType;
        public Faction Owner => TeamCache.Faction(owner.Value);

        public Sprite Icon { get; private set; }

        [SerializeField] private NetworkVariable<int> owner =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        /*  ITaggable Properties    */
        public string Key => "selectable";

        public Type Type => typeof(BuildingConstructionGhost);

        /*  ICommandable Properties */
        public Commandlet CurrentCommand => null;
        public int Count => 0;
        public List<string> Active => new List<string>();
        public List<Timer> Cooldowns => new List<Timer>();

        /*  IAttackable Properties  */
        public virtual int Health
        {
            get => currentHealth.Value;
            private set => currentHealth.Value = value;
        }

        public virtual int MaxHealth
        {
            get => maxHealth.Value;
            private set => maxHealth.Value = value;
        }

        [SerializeField] protected NetworkVariable<int> maxHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        [SerializeField] protected NetworkVariable<int> currentHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        private CostEntry[] _constructionCost;
        
        private int _healthPerConstructionPoint;
        private int _constructionRequired;
        private int _currentConstruction;

        private Building _buildingBeingConstructed;
        
        private Entity _entityComponent;
        private EventAgent _bus;
        private Transform _model;

        private GameObject[] _visionObjects = Array.Empty<GameObject>();

        private void Awake()
        {
            _bus = GetComponent<EventAgent>();
            _entityComponent = GetComponent<Entity>();
        }

        public virtual void InitializeGhost(Building buildingBeingConstructed, int constructionWorkRequired, params CostEntry[] constructionCost)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            
            UpdateProperties(buildingBeingConstructed, constructionWorkRequired, constructionCost);
            InstantiateChildObjects(buildingBeingConstructed);
        }

        protected void UpdateProperties(Building buildingBeingConstructed, int constructionWorkRequired, params CostEntry[] constructionCost)
        {
            _buildingBeingConstructed = buildingBeingConstructed;
            _constructionCost = constructionCost;
            _constructionRequired = constructionWorkRequired;
            MaxHealth = buildingBeingConstructed.MaxHealth;
            UnitType = buildingBeingConstructed.UnitType;
            Icon = buildingBeingConstructed.Icon;
            
            _healthPerConstructionPoint =
                Mathf.RoundToInt((float)buildingBeingConstructed.MaxHealth / _currentConstruction);
        }

        protected void InstantiateChildObjects(Building buildingBeingConstructed)
        {
            _model = Instantiate(buildingBeingConstructed.transform.Find("Model"), transform);
            
            Instantiate(buildingBeingConstructed.transform.Find("SelectionCircle"), transform);
            Instantiate(buildingBeingConstructed.transform.Find("MapSquare"), transform);
            Instantiate(buildingBeingConstructed.transform.Find("BarOrientation"), transform);
            Instantiate(buildingBeingConstructed.transform.Find("Collider"), transform);
            Instantiate(buildingBeingConstructed.transform.Find("SelectionCollider"), transform);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsServer)
            {
                AttachServerListeners();

                if (_currentConstruction > 0)
                {
                    float constructedProportion = (float)_currentConstruction / _constructionRequired;
                    currentHealth.Value = Mathf.RoundToInt(maxHealth.Value * constructedProportion);
                    
                }
                else
                {
                    currentHealth.Value = 0;
                }
            }

            if (NetworkManager.Singleton.IsClient)
            {
                AttachClientListeners();

                _model = transform.Find("Model(Clone)");

                if (_currentConstruction > 0)
                {
                    float constructedProportion = (float)_currentConstruction / _constructionRequired;
                    _model.localScale = Vector3.one * constructedProportion;
                }
                else
                {
                    _model.localScale = Vector3.zero;
                }
                
                _visionObjects = new[]
                {
                    _model.gameObject, 
                    transform.Find("SelectionCircle(Clone)").gameObject, 
                    transform.Find("MapSquare(Clone)").gameObject, 
                    transform.Find("BarOrientation(Clone)").gameObject, 
                    transform.Find("Collider(Clone)").gameObject,
                    transform.Find("SelectionCollider(Clone)").gameObject
                };
            }
        }

        private void AttachServerListeners() { }

        private void AttachClientListeners()
        {
            _bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);

            owner.OnValueChanged += (_, _)
                => _bus.Local(new UnitOwnerChangeEvent(_bus, this, Owner));
        }

        private void CancelConstruction()
        {
            _bus.Global(new UnitDeathEvent(_bus, this));

            foreach (CostEntry materialCost in _constructionCost)
            {
                Owner.GetResource(materialCost.key).Deposit(materialCost.amount);
            }

            Destroy(gameObject, 0.1f);
        }

        private void CompleteConstruction()
        {
            Building newBuilding = Instantiate(_buildingBeingConstructed, transform.position, transform.rotation);
            NetworkObject buildingNetworking = newBuilding.GetComponent<NetworkObject>();
            
            buildingNetworking.Spawn();
            newBuilding.SetOwner(Owner);
            
            _bus.Global(new UnitDeathEvent(_bus, this));
            Destroy(gameObject, 0.1f);
        }

        private void OnUnitInfoDisplayed(UnitInfoEvent @event)
        {
            if (!ReferenceEquals(@event.Unit, this)) return;
            
            HealthInfo info = @event.Info.Module<HealthInfo>("health");
            info.CurrentUnit = this;
        }

        private void OnVisionUpdate(EntityVisibleEvent @event)
        {
            bool visible = @event.Visible | GameVision.WasVisited(gameObject);

            foreach (GameObject hideable in _visionObjects)
            {
                hideable.SetActive(visible);
            }
        }

        public void Attack(int damage)
        {
            if (damage < 0)
            {
                _currentConstruction -= damage;

                float progress = (float)_currentConstruction / _constructionRequired;

                Health += _healthPerConstructionPoint * -damage;
                Health = Mathf.Clamp(Health, 0, MaxHealth);

                _model.localScale = Vector3.one * progress;
                
                _bus.Global(new UnitHurtEvent(_bus, this));

                if (progress >= 1f) CompleteConstruction();
                return;
            }
            
            if (Health <= 0) return;
            
            Health -= damage;
            _bus.Global(new UnitHurtEvent(_bus, this));

            if (Health <= 0)
            {
                _bus.Global(new UnitDeathEvent(_bus, this));
                Destroy(gameObject, 0.1f);
            }
        }

        // TODO: Convert into IAttackable & ISelectable extension method
        public Relationship GetRelationship(Faction other)
        {
            Relationship result = Owner.GetRelationship(other);
            return result;
        }

        public bool SetOwner(Faction player)
        {
            owner.Value = player.Id;
            return true;
        }

        public BuildingConstructionGhost Get() => this;

        public void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            if (order.Name == "cancelConstruction") CancelConstruction();
        }

        public CommandFactory Evaluate(ISelectable target) => CommandRegistry.Get("move");

        public void AutoCommand(ISelectable target) { }

        public string[] Commands() => new[]{ "cancelConstruction" };

        public bool CanCommand(string key) => key == "cancelConstruction";

        public void Select(bool status) => _bus.Local(new UnitSelectEvent(_bus, status));

        public void Hover(bool status)
        {
            if (Player.Main.HasSelected(this)) return;

            _bus.Local(new UnitHoverEvent(_bus, status));
        }
    }
}