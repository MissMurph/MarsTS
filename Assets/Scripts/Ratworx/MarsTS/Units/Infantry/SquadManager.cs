using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry
{
    public class SquadManager : NetworkBehaviour,
                                IEntityComponent<SquadManager>,
                                IEntityServerUpdate
    {
        public List<InfantryMember> Members {
            get
            {
                var output = new List<InfantryMember>();

                foreach (MemberEntry unitEntry in _members.Values) {
                    output.Add(unitEntry.Member);
                }

                return output;
            }
        }

        private readonly Dictionary<string, MemberEntry> _members = new Dictionary<string, MemberEntry>();

        private Vector3 _squadAvgPos;
        
        private Entity _entity;
        private EventAgent _eventAgent;
        private SquadVisionParser _squadVisibility;

        protected virtual void Awake() {
            _entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();
            _squadVisibility = GetComponent<SquadVisionParser>();

            _entity.OnEntityInit += OnSquadEntityInit;
        }

        public void UpdateServer() {
            if (_members.Count <= 0) return;
            
            _squadAvgPos = Vector3.zero;

            foreach (MemberEntry entry in _members.Values)
            {
                if (!entry.Member) continue;
                
                _squadAvgPos += entry.Member.transform.position;
            }

            _squadAvgPos /= _members.Count;

            transform.position = _squadAvgPos;
        }

        private void OnSquadEntityInit(Phase phase) {
            if (!NetworkManager.Singleton.IsServer) return;
            if (phase == Phase.Pre) return;

            SpawnAndInitializeMembers();
        }

        private void SpawnAndInitializeMembers() {
            if (!Registry.Registry.TryGetPrefab("misc:spawner", out GameObject prefab)) {
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
            for (int x = -1; x < 1; x++)
            for (int y = -1; y < 1; y++) {
                if (x == 0 && y == 0) continue;

                Vector3 pos = new Vector3(
                    spawnPos.x + x * memberHalfExtents.x * 4,
                    spawnPos.y,
                    spawnPos.z + y * memberHalfExtents.z * 4
                );

                if (Physics.CheckBox(pos, memberHalfExtents)) continue;

                spawner.transform.position = pos;

                InfantryMember member = spawner.SpawnEntity().GetComponent<InfantryMember>();

                AttachMemberInitListener(member);
            }
        }

        private void AttachMemberInitListener(InfantryMember unit) {
            Entity memberEntity = unit.GetComponent<Entity>();

            memberEntity.OnEntityInit += phase =>
            {
                if (phase == Phase.Pre)
                    return;

                RegisterMember(unit);
            };
        }

        protected virtual void RegisterMember(InfantryMember member) {
            MemberEntry newEntry = new MemberEntry();

            newEntry.Key = member.name;
            newEntry.Member = member;
            newEntry.Bus = member.GetComponent<EventAgent>();

            _members[newEntry.Key] = newEntry;

            member.SetOwner(Owner);
            member.SetSquad(this);

            InstantiateDummyColliders(member);

            if (NetworkManager.Singleton.IsServer) {
                AttachMemberServerListeners(member);
                RegisterMemberClientRpc(newEntry.Key);
            }

            if (NetworkManager.Singleton.IsClient)
                AttachMemberClientListeners(member);

            if (!_isInitialized) _isInitialized = true;
        }

        [Rpc(SendTo.NotServer)]
        private void RegisterMemberClientRpc(string entityName) {
            if (NetworkManager.Singleton.IsServer)
                return;

            if (!EntityCache.TryGetEntityComponent(entityName, out InfantryMember member)) {
                Debug.LogError($"[CLIENT] Failed to find Entity {entityName} for registering infantry member!");
                return;
            }

            RegisterMember(member);
        }

        protected virtual void AttachMemberServerListeners(InfantryMember unit) {
            EventAgent unitEvents = unit.GetComponent<EventAgent>();

            unitEvents.AddListener<UnitDeathEvent>(DeregisterMember);

            _eventAgent.PostLocal(new SquadRegisterEvent(_eventAgent, this, unit));
        }

        private void AttachMemberClientListeners(InfantryMember unit) {
            EventAgent unitEvents = unit.GetComponent<EventAgent>();

            unitEvents.AddListener<UnitHurtEvent>(ForwardHurtEvent);
        }

        private void InstantiateDummyColliders(InfantryMember member) {
            SquadColliderTracker selectCollider = Instantiate(_selectionColliderPrefab, transform)
                .GetComponent<SquadColliderTracker>();
            selectCollider.Init(member);

            SquadColliderTracker detectCollider = Instantiate(_dummyColliderPrefab, transform)
                .GetComponent<SquadColliderTracker>();
            detectCollider.Init(member);
        }

        private void DeregisterMember(UnitDeathEvent _event) {
            MemberEntry deadEntry = _members[_event.Unit.GameObject.name];

            _members.Remove(deadEntry.Key);

            if (_members.Count <= 0) {
                _eventAgent.PostGlobal(new UnitDeathEvent(_eventAgent, this));
                Destroy(gameObject);
            }
        }

        public override void OnDestroy() {
            _members.Clear();
        }

        protected class MemberEntry
        {
            public string Key;
            public InfantryMember Member;
            public EventAgent Bus;
        }

        public SquadManager Get() => this;

        public string Key => "squad_manager";
    }
}