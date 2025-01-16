using System;
using System.Collections.Generic;
using MarsTS.Commands;
using MarsTS.Editor;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Prefabs;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Units
{
    public class InfantrySquad : NetworkBehaviour,
        ISelectable,
        ITaggable<InfantrySquad>,
        ICommandable,
        IAttackable
    {
        public GameObject GameObject => gameObject;
        public IUnit Unit => this;

        /*	ISelectable Properties	*/

        public int Id => _entityComponent.Id;

        public string UnitType => _type;

        public string RegistryKey => "unit:" + UnitType;

        public Faction Owner => _owner;

        public Sprite Icon => _icon;

        [FormerlySerializedAs("icon")] [SerializeField]
        private Sprite _icon;

        [FormerlySerializedAs("type")] [SerializeField]
        private string _type;

        [FormerlySerializedAs("owner")] [SerializeField]
        protected Faction _owner;

        /*	ITaggable Properties	*/

        public string Key => "selectable";

        public Type Type => typeof(InfantrySquad);

        /*	ICommandable Properties	*/

        public Commandlet CurrentCommand => _commands.Current;

        public Commandlet[] CommandQueue => _commands.Queue;

        public List<string> Active => _commands.Active;

        public List<Timer> Cooldowns => _commands.Cooldowns;

        public int Count => _commands.Count;

        [FormerlySerializedAs("boundCommands")] [SerializeField]
        protected string[] _boundCommands;

        protected CommandQueue _commands;

        /*	InfantrySquad Fields	*/

        public List<ISelectable> Members
        {
            get
            {
                var output = new List<ISelectable>();

                foreach (MemberEntry unitEntry in _members.Values)
                {
                    output.Add(unitEntry.Member);
                }

                return output;
            }
        }

        protected readonly Dictionary<string, MemberEntry> _members = new Dictionary<string, MemberEntry>();

        protected Entity _entityComponent;

        [FormerlySerializedAs("maxMembers")] [SerializeField]
        protected int _maxMembers;

        [FormerlySerializedAs("startingMembers")] [SerializeField]
        protected InfantryMember[] _startingMembers;

        [FormerlySerializedAs("selectionColliderPrefab")] [SerializeField]
        protected GameObject _selectionColliderPrefab;

        [FormerlySerializedAs("dummyColliderPrefab")] [SerializeField]
        protected GameObject _dummyColliderPrefab;

        [SerializeField] protected InfantryMember _memberPrefab;

        protected EventAgent _bus;

        protected Vector3 _squadAvgPos;

        protected SquadVisionParser _squadVisibility;

        protected EntitySpawner _spawnerPrefab;

        public int Health
        {
            get
            {
                int current = 0;

                foreach (MemberEntry unitEntry in _members.Values)
                {
                    current += unitEntry.Member.Health;
                }

                return current;
            }
        }

        public int MaxHealth
        {
            get
            {
                int max = 0;

                foreach (MemberEntry unitEntry in _members.Values)
                {
                    max += unitEntry.Member.MaxHealth;
                }

                return max;
            }
        }

        //protected List<GameObject> dummysToDestroy = new List<GameObject>();

        protected virtual void Awake()
        {
            _entityComponent = GetComponent<Entity>();
            _bus = GetComponent<EventAgent>();
            _commands = GetComponent<CommandQueue>();
            _squadVisibility = GetComponent<SquadVisionParser>();

            _bus.AddListener<EntityInitEvent>(OnEntityInit);
        }

        public override void OnNetworkSpawn()
        {
        }

        private void SpawnAndInitializeMembers()
        {
            if (!Registry.TryGetPrefab("misc:spawner", out GameObject prefab))
            {
                Debug.LogError($"{typeof(InfantrySquad)} {gameObject.name} Couldn't find misc:spawner prefab!");
                return;
            }

            _spawnerPrefab = prefab.GetComponent<EntitySpawner>();
            Vector3 memberHalfExtents =
                _memberPrefab.transform.Find("GroundCollider").GetComponent<Collider>().bounds.extents;

            EntitySpawner spawner = Instantiate(_spawnerPrefab, transform.position, transform.rotation);
            spawner.SetEntity(_memberPrefab.gameObject);

            InfantryMember firstMember = spawner.SpawnEntity().GetComponent<InfantryMember>();
            RegisterMember(firstMember);

            for (int i = 1; i < _maxMembers - _members.Count; i++)
            for (int x = -1; x < 1; x++)
            for (int y = -1; y < 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3 pos = new Vector3(
                    transform.position.x + x * memberHalfExtents.x * 2,
                    transform.position.y,
                    transform.position.z + y * memberHalfExtents.z * 2
                );

                if (Physics.CheckBox(pos, memberHalfExtents)) continue;

                spawner.transform.position = pos;

                InfantryMember member = spawner.SpawnEntity().GetComponent<InfantryMember>();
                RegisterMember(member);
            }
        }

        private void OnEntityInit(EntityInitEvent evnt)
        {
            if (evnt.Phase == Phase.Pre) return;
            if (!NetworkManager.Singleton.IsServer) return;

            foreach (EntitySpawner spawner in GetComponentsInChildren<EntitySpawner>())
            {
                spawner.SetEntity(_memberPrefab.gameObject);
                Entity entity = spawner.SpawnEntity();
                RegisterMember(entity.GetComponent<InfantryMember>());
            }

            SpawnAndInitializeMembers();
        }

        protected virtual void Start()
        {
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
            _bus.AddListener<CommandStartEvent>(ExecuteOrder);
        }

        protected virtual void Update()
        {
            _squadAvgPos = Vector3.zero;

            if (_members.Count <= 0) return;

            foreach (MemberEntry entry in _members.Values)
            {
                entry.SelectionCollider.transform.position = entry.Member.transform.position;
                entry.DetectableCollider.transform.position = entry.Member.transform.position;
                _squadAvgPos += entry.Member.transform.position;
            }

            _squadAvgPos /= _members.Count;

            transform.position = _squadAvgPos;
        }

        protected virtual void RegisterMember(InfantryMember unit)
        {
            unit.SetOwner(_owner);
            unit.squad = this;

            EventAgent unitEvents = unit.GetComponent<EventAgent>();
            unitEvents.AddListener<UnitDeathEvent>(OnMemberDeath);
            unitEvents.AddListener<UnitHurtEvent>(OnMemberHurt);
            unitEvents.AddListener<EntityInitEvent>(OnMemberInit);
            unitEvents.AddListener<EntityVisibleEvent>(OnMemberVisionUpdate);

            _bus.Local(new SquadRegisterEvent(_bus, this, unit));
        }

        protected virtual void OnMemberInit(EntityInitEvent _event)
        {
            if (_event.Phase == Phase.Post) return;
            MemberEntry newEntry = new MemberEntry();

            Transform newSelectionCollider = Instantiate(_selectionColliderPrefab, transform).transform;
            newSelectionCollider.position = _event.ParentEntity.transform.position;

            Transform dummyCollider = Instantiate(_dummyColliderPrefab, transform).transform;
            dummyCollider.position = _event.ParentEntity.transform.position;

            newEntry.Key = _event.ParentEntity.name;
            newEntry.Member = _event.ParentEntity.Get<InfantryMember>("selectable");
            newEntry.SelectionCollider = newSelectionCollider;
            newEntry.DetectableCollider = dummyCollider;
            newEntry.Bus = _event.Source;

            _members[newEntry.Key] = newEntry;
        }

        private void OnMemberHurt(UnitHurtEvent _event)
        {
            UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, _event.Damage);
            hurtEvent.Phase = Phase.Post;
            _bus.Global(hurtEvent);
        }

        private void OnMemberDeath(UnitDeathEvent _event)
        {
            MemberEntry deadEntry = _members[_event.Unit.GameObject.name];

            _members.Remove(deadEntry.Key);

            Destroy(deadEntry.SelectionCollider.gameObject);
            deadEntry.DetectableCollider.position = Vector3.down * 1000f;
            //dummysToDestroy.Add(deadEntry.detectableCollider.gameObject);


            if (_members.Count <= 0)
            {
                _bus.Global(new UnitDeathEvent(_bus, this));
                Destroy(gameObject);
            }
            else
            {
                foreach (MemberEntry entry in _members.Values)
                {
                    entry.DetectableCollider.transform.position = Vector3.down * 1000f;
                }
            }
        }

        private void OnMemberVisionUpdate(EntityVisibleEvent _event)
        {
            if (_members.TryGetValue(_event.UnitName, out MemberEntry entry))
                entry.SelectionCollider.gameObject.SetActive(_event.Visible);
            //entry.detectableCollider.gameObject.SetActive(_event.Visible);
        }

        protected virtual void ExecuteOrder(CommandStartEvent _event)
        {
            foreach (MemberEntry entry in _members.Values)
            {
                entry.Member.Order(_event.Command, false);
            }
        }

        public virtual string[] Commands() => _boundCommands;

        public virtual void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            switch (order.Name)
            {
                case "move":

                    break;
                case "stop":

                    break;
                default:
                    return;
            }

            if (inclusive) _commands.Enqueue(order);
            else _commands.Execute(order);
        }

        public virtual CommandFactory Evaluate(ISelectable target) => CommandRegistry.Get("move");

        public virtual void AutoCommand(ISelectable target)
        {
            throw new NotImplementedException();
        }

        public InfantrySquad Get() => this;

        public Relationship GetRelationship(Faction player) => Owner.GetRelationship(player);

        public void Hover(bool status)
        {
            foreach (MemberEntry entry in _members.Values)
            {
                entry.Member.Hover(status);
            }
        }

        public void Select(bool status)
        {
            foreach (MemberEntry entry in _members.Values)
            {
                entry.Member.Select(status);
            }
        }

        public bool SetOwner(Faction player)
        {
            _owner = player;
            return true;
        }

        protected virtual void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                HealthInfo info = _event.Info.Module<HealthInfo>("health");
                info.CurrentUnit = this;
            }
        }

        public void Attack(int damage)
        {
            throw new NotImplementedException();
        }

        public virtual bool CanCommand(string key)
        {
            bool canUse = false;

            for (int i = 0; i < _boundCommands.Length; i++)
            {
                if (_boundCommands[i] == key) break;

                if (i >= _boundCommands.Length - 1) return false;
            }

            if (_commands.CanCommand(key)) canUse = true;
            //if (production.CanCommand(key)) canUse = true;

            return canUse;
        }

        protected class MemberEntry
        {
            public string Key;
            public InfantryMember Member;
            public Transform SelectionCollider;
            public Transform DetectableCollider;
            public EventAgent Bus;
        }
    }
}