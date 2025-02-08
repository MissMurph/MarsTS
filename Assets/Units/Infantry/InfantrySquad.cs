using System;
using System.Collections.Generic;
using System.Linq;
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

        public Faction Owner => TeamCache.Faction(_owner);
        
        public Sprite Icon => _icon;

        [FormerlySerializedAs("icon")] [SerializeField]
        private Sprite _icon;

        [FormerlySerializedAs("type")] [SerializeField]
        private string _type;

        [SerializeField] protected int _owner;

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

        public int Health => _members.Values.Sum(unitEntry => unitEntry.Member.Health);

        public int MaxHealth => _memberPrefab.MaxHealth * _maxMembers;

        protected virtual void Awake()
        {
            _entityComponent = GetComponent<Entity>();
            _bus = GetComponent<EventAgent>();
            _commands = GetComponent<CommandQueue>();
            _squadVisibility = GetComponent<SquadVisionParser>();
            
            _entityComponent.OnEntityInit += OnSquadEntityInit;
        }

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsServer) AttachServerListeners();
            if (NetworkManager.Singleton.IsClient) AttachClientListeners();
        }

        protected virtual void AttachServerListeners()
        {
            _bus.AddListener<CommandStartEvent>(ExecuteOrder);
        }

        protected virtual void AttachClientListeners()
        {
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
        }

        private void OnSquadEntityInit(Phase phase)
        {
            if (!NetworkManager.Singleton.IsServer) return;
            if (phase == Phase.Pre) return;

            SpawnAndInitializeMembers();
        }

        private void SpawnAndInitializeMembers()
        {
            if (!Registry.TryGetPrefab("misc:spawner", out GameObject prefab))
            {
                Debug.LogError($"{typeof(InfantrySquad)} {gameObject.name} Couldn't find misc:spawner prefab!");
                return;
            }

            _spawnerPrefab = prefab.GetComponent<EntitySpawner>();

            EntitySpawner spawner = Instantiate(_spawnerPrefab, transform.position, transform.rotation);
            spawner.SetDeferredSpawn(true);
            spawner.SetEntity(_memberPrefab.gameObject);
            spawner.SetOwner(Owner.Id);

            //We capture pos here as squad will move around while instantiating
            Vector3 spawnPos = transform.position;
            
            InfantryMember firstMember = spawner.SpawnEntity().GetComponent<InfantryMember>();

            AttachMemberInitListener(firstMember);

            Vector3 memberHalfExtents =
                firstMember.transform
                    .Find("GroundCollider")
                    .GetComponent<BoxCollider>()
                    .size;

            for (int i = 1; i < _maxMembers - _members.Count - 1; i++)
            {
                for (int x = -1; x < 1; x++)
                {
                    for (int y = -1; y < 1; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        Vector3 pos = new Vector3(
                            spawnPos.x + (x * memberHalfExtents.x * 4),
                            spawnPos.y,
                            spawnPos.z + (y * memberHalfExtents.z * 4)
                        );

                        if (Physics.CheckBox(pos, memberHalfExtents))
                        {
                            continue;
                        }

                        spawner.transform.position = pos;

                        InfantryMember member = spawner.SpawnEntity().GetComponent<InfantryMember>();

                        AttachMemberInitListener(member);
                    }
                }
            }
        }

        protected virtual void Update()
        {
            _squadAvgPos = Vector3.zero;

            if (_members.Count <= 0) return;

            foreach (MemberEntry entry in _members.Values)
            {
                if (!entry.Member) continue;
                
                _squadAvgPos += entry.Member.transform.position;
            }

            _squadAvgPos /= _members.Count;

            transform.position = _squadAvgPos;
        }

        private void AttachMemberInitListener(InfantryMember unit)
        {
            Entity memberEntity = unit.GetComponent<Entity>();

            memberEntity.OnEntityInit += phase =>
            {
                if (phase == Phase.Pre) 
                    return;
                
                RegisterMember(unit);
            };
        }

        protected virtual void RegisterMember(InfantryMember member)
        {
            MemberEntry newEntry = new MemberEntry();

            newEntry.Key = member.name;
            newEntry.Member = member;
            newEntry.Bus = member.GetComponent<EventAgent>();

            _members[newEntry.Key] = newEntry;
            
            member.SetOwner(Owner);
            member.SetSquad(this);
            
            InstantiateDummyColliders(member);

            if (NetworkManager.Singleton.IsServer)
            {
                AttachMemberServerListeners(member);
                RegisterMemberClientRpc(newEntry.Key);
            }

            if (NetworkManager.Singleton.IsClient) 
                AttachMemberClientListeners(member);
        }

        [Rpc(SendTo.NotServer)]
        private void RegisterMemberClientRpc(string entityName)
        {
            if (!EntityCache.TryGet(entityName, out InfantryMember member))
            {
                Debug.LogError($"[CLIENT] Failed to find Entity {entityName} for registering infantry member!");
                return;
            }
            
            RegisterMember(member);
        }

        protected virtual void AttachMemberServerListeners(InfantryMember unit)
        {
            EventAgent unitEvents = unit.GetComponent<EventAgent>();
            
            unitEvents.AddListener<UnitDeathEvent>(DeregisterMember);

            _bus.Local(new SquadRegisterEvent(_bus, this, unit));
        }

        private void AttachMemberClientListeners(InfantryMember unit)
        {
            EventAgent unitEvents = unit.GetComponent<EventAgent>();

            unitEvents.AddListener<UnitHurtEvent>(ForwardHurtEvent);
        }

        private void InstantiateDummyColliders(InfantryMember member)
        {
            SquadDummyColliderTracker selectCollider = Instantiate(_selectionColliderPrefab, transform)
                .GetComponent<SquadDummyColliderTracker>();
            selectCollider.Init(member);
            
            SquadDummyColliderTracker detectCollider = Instantiate(_dummyColliderPrefab, transform)
                .GetComponent<SquadDummyColliderTracker>();
            detectCollider.Init(member);
        }

        private void ForwardHurtEvent(UnitHurtEvent _event)
        {
            UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, _event.Damage);
            hurtEvent.Phase = Phase.Post;
            _bus.Global(hurtEvent);
        }

        private void DeregisterMember(UnitDeathEvent _event)
        {
            MemberEntry deadEntry = _members[_event.Unit.GameObject.name];

            _members.Remove(deadEntry.Key);

            if (_members.Count <= 0)
            {
                _bus.Global(new UnitDeathEvent(_bus, this));
                Destroy(gameObject);
            }
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
            if (!NetworkManager.Singleton.IsServer) return false;
            
            _owner = player.Id;
            SetOwnerClientRpc(_owner);
            _bus.Global(new UnitOwnerChangeEvent(_bus, this, Owner));
            
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId)
        {
            _owner = newId;
            _bus.Global(new UnitOwnerChangeEvent(_bus, this, Owner));
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

        public override void OnDestroy()
        {
            _members.Clear();
        }

        protected class MemberEntry
        {
            public string Key;
            public InfantryMember Member;
            public EventAgent Bus;
        }
    }
}