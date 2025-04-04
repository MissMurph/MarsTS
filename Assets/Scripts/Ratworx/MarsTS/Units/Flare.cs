using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units {
    public class Flare : NetworkBehaviour, 
        ISelectable, 
        IEntityComponent<Flare>, 
        IAttackable 
    {
        [FormerlySerializedAs("lifeTime")]
        [SerializeField]
        private float _lifeTime;

        private float _currentLifeTime;

        private EventAgent _bus;

        [FormerlySerializedAs("hideables")]
        [SerializeField]
        private GameObject[] _hideables;

        public GameObject GameObject => gameObject;
        public IUnitInterface UnitInterface => this;

        /*	IAttackable Properties	*/

        public int Health
        {
            get => _currentHealth.Value;
            private set => _currentHealth.Value = value;
        }

        public int MaxHealth => maxHealth;

        private readonly int maxHealth = 100;

        [SerializeField]
        private NetworkVariable<int> _currentHealth = new (writePerm: NetworkVariableWritePermission.Server);

        /*	ISelectable Properties	*/

        public int Id => _entityComponent.Id;

        public string UnitType => _type;

        public string RegistryKey => "misc:" + UnitType;

        public Sprite Icon => _icon;

        public Faction Owner => _owner;

        [FormerlySerializedAs("owner")]
        [SerializeField]
        private Faction _owner;

        [FormerlySerializedAs("type")]
        [SerializeField]
        private string _type;

        [FormerlySerializedAs("icon")]
        [SerializeField]
        private Sprite _icon;

        /*	ITaggable Properties	*/

        public string Key => "selectable";

        public Type Type => typeof(AbstractUnit);

        private ISelectable _parent;

        private Entity _entityComponent;

        private void Awake() {
            _bus = GetComponent<EventAgent>();
            _entityComponent = GetComponent<Entity>();

            Health = maxHealth;
            _currentLifeTime = _lifeTime;
        }

        private void Start() {
            _bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
            EventBus.AddListener<VisionInitEvent>(OnVisionInit);
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();

            _currentHealth.OnValueChanged += OnHurt;
        }

        private void OnVisionInit(VisionInitEvent _event) {
            bool visible = GameVision.IsVisible(gameObject);

            foreach (GameObject hideable in _hideables) {
                hideable.SetActive(visible);
            }
        }

        private void Update() {
            if (!NetworkManager.Singleton.IsServer) return;
            
            int previousHealth = Health;
            _currentLifeTime -= Time.deltaTime;
            Health = Mathf.RoundToInt(maxHealth * (_currentLifeTime / _lifeTime));

            UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, previousHealth - Health);
            hurtEvent.Phase = Phase.Post;
            _bus.Global(hurtEvent);

            if (_currentLifeTime <= 0) {
                _bus.Global(new UnitDeathEvent(_bus, this));
                Destroy(gameObject);
            }
        }

        private void OnVisionUpdate(EntityVisibleEvent _event) {
            foreach (GameObject hideable in _hideables) {
                hideable.SetActive(_event.Visible);
            }
        }

        public void Select(bool status) {
            _bus.Local(new UnitSelectEvent(_bus, status));
        }

        public void Hover(bool status) {
            //These are seperated due to the Player Selection Check
            if (status)
                _bus.Local(new UnitHoverEvent(_bus, status));
            else if (!Player.Player.HasSelected(this)) _bus.Local(new UnitHoverEvent(_bus, status));
        }

        public Relationship GetRelationship(Faction other) => Owner.GetRelationship(other);

        public bool SetOwner(Faction player) {
            if (!NetworkManager.Singleton.IsServer) return false;
            
            _owner = player;
            SetOwnerClientRpc(_owner.Id);
            _bus.Global(new UnitOwnerChangeEvent(_bus, this, Owner));
            
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId) {
            _owner = TeamCache.Faction(newId);
            _bus.Global(new UnitOwnerChangeEvent(_bus, this, Owner));
        }

        public Flare Get() => this;

        private void OnUnitInfoDisplayed(UnitInfoEvent _event) {
            if (ReferenceEquals(_event.Unit, this)) {
                HealthInfo info = _event.Info.Module<HealthInfo>("health");
                info.CurrentUnit = this;
            }
        }

        public void Attack(int damage) {
            if (Health <= 0) 
                return;
            
            if (damage < 0 && Health >= MaxHealth) 
                return;

            UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, damage);
            hurtEvent.Phase = Phase.Pre;
            _bus.Global(hurtEvent);

            damage = hurtEvent.Damage;
            Health -= damage;

            hurtEvent.Phase = Phase.Post;
            _bus.Global(hurtEvent);

            if (Health <= 0)
            {
                _bus.Global(new UnitDeathEvent(_bus, this));
                Destroy(gameObject, 0.1f);
            }
        }

        private void OnHurt(int oldHealth, int newHealth) {
            if (Health <= 0)
            {
                _bus.Global(new UnitDeathEvent(_bus, this));

                if (NetworkManager.Singleton.IsServer)
                    Destroy(gameObject, 0.1f);
            }
            else
            {
                UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, oldHealth - newHealth);
                hurtEvent.Phase = Phase.Post;
                _bus.Global(hurtEvent);
            }
        }
    }
}