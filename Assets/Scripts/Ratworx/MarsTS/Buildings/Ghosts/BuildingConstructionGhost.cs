using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings.Ghosts
{
    public class BuildingConstructionGhost : NetworkBehaviour
    {
        private ResourceCost[] _constructionCost;
        
        private int _healthPerConstructionPoint;

        protected int ConstructionRequired
        {
            get => constructionRequired.Value;
            set => constructionRequired.Value = value;
        }

        protected int CurrentConstruction
        {
            get => currentConstruction.Value;
            set => currentConstruction.Value = value;
        }
        
        [SerializeField] private NetworkVariable<int> constructionRequired =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        [SerializeField] private NetworkVariable<int> currentConstruction =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

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

        public virtual void InitializeGhost(string buildingKey, int constructionWorkRequired, params ResourceCost[] constructionCost)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            
            UpdateProperties(buildingKey, constructionWorkRequired, constructionCost);
            InstantiateChildObjects();
            
            InitializeGhostClientRpc(buildingKey);

            _bus.PostLocal(new UnitInitEvent(this, _bus));
        }

        protected void UpdateProperties(string buildingKey, int constructionWorkRequired, params ResourceCost[] constructionCost)
        { 
            Registry.Registry.TryGetObject($"building:{buildingKey}", out Building buildingBeingConstructed);

            ConstructionRequired = constructionWorkRequired;
            MaxHealth = buildingBeingConstructed.MaxHealth;
            
            if (CurrentConstruction > 0)
            {
                float constructedProportion = (float)CurrentConstruction / ConstructionRequired;
                Health = Mathf.RoundToInt(maxHealth.Value * constructedProportion);
            }
            else
            {
                Health = 1;
            }
            
            _buildingBeingConstructed = buildingBeingConstructed;
            _constructionCost = constructionCost;
            UnitType = buildingBeingConstructed.UnitType;
            Icon = buildingBeingConstructed.Icon;
            
            _healthPerConstructionPoint = 
                Mathf.RoundToInt((float)buildingBeingConstructed.MaxHealth / ConstructionRequired);
        }

        [Rpc(SendTo.NotServer)]
        private void InitializeGhostClientRpc(string buildingKey)
        {
            UpdatePropertiesClient(buildingKey);
            InstantiateChildObjects();
        }

        private void UpdatePropertiesClient(string buildingKey)
        {
            Registry.Registry.TryGetObject($"building:{buildingKey}", out Building buildingBeingConstructed);
            
            _buildingBeingConstructed = buildingBeingConstructed;
            UnitType = buildingBeingConstructed.UnitType;
            Icon = buildingBeingConstructed.Icon;
        }

        private void InstantiateChildObjects()
        {
            _model = Instantiate(_buildingBeingConstructed.transform.Find("Model"), transform);

            var selectionCircle = Instantiate(_buildingBeingConstructed.transform.Find("SelectionCircle"), transform);
            var mapSquare = Instantiate(_buildingBeingConstructed.transform.Find("MapSquare"), transform);
            var barOrientation = Instantiate(_buildingBeingConstructed.transform.Find("BarOrientation"), transform);
            Instantiate(_buildingBeingConstructed.transform.Find("Collider"), transform);
            var selectionCollider = Instantiate(_buildingBeingConstructed.transform.Find("SelectionCollider"), transform);

            if (CurrentConstruction > 0)
            {
                float constructedProportion = (float)CurrentConstruction / ConstructionRequired;
                _model.localScale = Vector3.one * constructedProportion;
            }
            else
            {
                _model.localScale = Vector3.zero;
            }
                
            _visionObjects = new[]
            {
                _model.gameObject, 
                selectionCircle.gameObject, 
                mapSquare.gameObject, 
                barOrientation.gameObject, 
                //hitCollider.gameObject,
                selectionCollider.gameObject
            };
            
            foreach (GameObject visionObject in _visionObjects)
            {
                visionObject.SetActive(false);
            }
        }

        private void CancelConstruction()
        {
            _bus.PostGlobal(new UnitDeathEvent(_bus, this));

            foreach (ResourceCost materialCost in _constructionCost)
            {
                Owner.GetResource(materialCost.key).Deposit(materialCost.amount);
            }

            Destroy(gameObject, 0.1f);
        }

        private void CompleteConstruction()
        {
            SendCompletionClientEventRpc();
            
            Building newBuilding = Instantiate(_buildingBeingConstructed, transform.position, transform.rotation);
            NetworkObject buildingNetworking = newBuilding.GetComponent<NetworkObject>();
            
            buildingNetworking.Spawn();
            newBuilding.SetOwner(Owner);
            
            _bus.PostGlobal(new UnitDeathEvent(_bus, this));
            Destroy(gameObject, 0.1f);
        }

        [Rpc(SendTo.NotServer)]
        private void SendCompletionClientEventRpc()
        {
            _bus.PostGlobal(new UnitDeathEvent(_bus, this));
        }

        public void Attack(int damage)
        {
            UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, damage);
            hurtEvent.Phase = Phase.Pre;
            _bus.PostGlobal(hurtEvent);
            
            damage = hurtEvent.Damage;

            if (damage < 0)
            {
                CurrentConstruction -= damage;

                float progress = (float)CurrentConstruction / ConstructionRequired;

                Health += _healthPerConstructionPoint * -damage;
                Health = Mathf.Clamp(Health, 0, MaxHealth);

                _model.localScale = Vector3.one * progress;

                hurtEvent.Phase = Phase.Post;
                _bus.PostGlobal(hurtEvent);

                if (progress >= 1f) CompleteConstruction();
                return;
            }
            
            if (Health <= 0) return;
            
            Health -= damage;

            hurtEvent.Phase = Phase.Post;
            _bus.PostGlobal(hurtEvent);

            if (Health <= 0)
            {
                _bus.PostGlobal(new UnitDeathEvent(_bus, this));
                Destroy(gameObject, 0.1f);
            }
        }
    }
}