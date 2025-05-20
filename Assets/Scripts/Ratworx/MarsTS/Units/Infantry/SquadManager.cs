using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Units.Squads;
using Ratworx.MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Infantry
{
    public class SquadManager : NetworkBehaviour,
                                IEntityComponent<SquadManager>,
                                IEntityServerUpdate
    {
        /// <remarks><c>bool</c> value reflects if the member is joining or leaving the squad</remarks>
        public event Action<SquadMemberEntry, bool> OnSquadMembershipChanged;
        
        public List<InfantryMembership> Members {
            get
            {
                var output = new List<InfantryMembership>();

                foreach (SquadMemberEntry unitEntry in _members.Values) {
                    output.Add(unitEntry.Membership);
                }

                return output;
            }
        }

        public List<SquadMemberEntry> MemberEntries => _members.Values.ToList();
        public int MaxMembers => _maxMembers;

        [SerializeField] private int _maxMembers;
        [SerializeField] private InfantryMembership[] _startingMembers;
        [SerializeField] private GameObject _selectionColliderPrefab;
        [SerializeField] private GameObject _dummyColliderPrefab;
        [FormerlySerializedAs("_memberPrefab")] [SerializeField] private InfantryMembership _membershipPrefab;

        private readonly Dictionary<int, SquadMemberEntry> _members = new Dictionary<int, SquadMemberEntry>();

        private Vector3 _squadAvgPos;
        
        private Entity _entity;
        private EventAgent _eventAgent;
        private SquadVisionParser _squadVisibility;
        private EntitySpawner _spawnerPrefab;

        private void Awake() {
            _entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();
            _squadVisibility = GetComponent<SquadVisionParser>();

            _entity.OnEntityInit += OnSquadEntityInit;
        }

        public void UpdateServer() {
            if (_members.Count <= 0) return;
            
            _squadAvgPos = Vector3.zero;

            foreach (SquadMemberEntry entry in _members.Values)
            {
                if (!entry.Membership) continue;
                
                _squadAvgPos += entry.Membership.transform.position;
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
            spawner.SetEntity(_membershipPrefab.gameObject);
            // spawner.SetOwner(Owner.Id);

            //We capture pos here as squad will move around while instantiating
            Vector3 spawnPos = transform.position;

            InfantryMembership firstMembership = spawner.SpawnEntity().GetComponent<InfantryMembership>();

            AttachMemberInitListener(firstMembership);

            Vector3 memberHalfExtents =
                firstMembership.transform
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

                InfantryMembership membership = spawner.SpawnEntity().GetComponent<InfantryMembership>();

                AttachMemberInitListener(membership);
            }
        }

        private void AttachMemberInitListener(InfantryMembership unit) {
            Entity memberEntity = unit.GetComponent<Entity>();

            memberEntity.OnEntityInit += phase =>
            {
                if (phase == Phase.Pre)
                    return;

                RegisterMember(memberEntity);
            };
        }

        protected virtual void RegisterMember(Entity memberEntity) {
            SquadMemberEntry newEntry = new SquadMemberEntry();

            newEntry.InstanceId = memberEntity.Id;
            newEntry.Entity = memberEntity;
            memberEntity.TryGetEntityComponent(out newEntry.Membership);
            memberEntity.TryGetEntityComponent(out newEntry.EventAgent);
            memberEntity.TryGetEntityComponent(out newEntry.Ownership);
            memberEntity.TryGetEntityComponent(out newEntry.Selection);
            memberEntity.TryGetEntityComponent(out newEntry.CommandQueue);
            
            _members[newEntry.InstanceId] = newEntry;

            newEntry.Membership.SetSquad(this);
            
            InstantiateDummyColliders(newEntry.Membership);

            if (NetworkManager.Singleton.IsServer) {
                AttachMemberServerListeners(newEntry.Membership);
                RegisterMemberClientRpc(newEntry.InstanceId);
            }

            // if (!_isInitialized) _isInitialized = true;
        }

        [Rpc(SendTo.NotServer)]
        private void RegisterMemberClientRpc(int instanceId) {
            if (NetworkManager.Singleton.IsServer)
                return;

            if (!EntityCache.TryGetEntity(instanceId, out Entity entity)) {
                RatLogger.Error?.Log($"[CLIENT] Failed to find Entity {instanceId} for registering infantry member!");
                return;
            }
            
            if (!entity.TryGetEntityComponent(out InfantryMembership _)) {
                RatLogger.Error?.Log($"[CLIENT] Failed to find {nameof(InfantryMembership)} on {entity.name} for registering infantry member!");
                return;
            }

            RegisterMember(entity);
        }

        protected virtual void AttachMemberServerListeners(InfantryMembership unit) {
            EventAgent unitEvents = unit.GetComponent<EventAgent>();

            unitEvents.AddListener<UnitDeathEvent>(DeregisterMember);

            // _eventAgent.PostLocal(new SquadRegisterEvent(_eventAgent, this, unit));
        }

        private void InstantiateDummyColliders(InfantryMembership membership) {
            SquadColliderTracker selectCollider = Instantiate(_selectionColliderPrefab, transform)
                .GetComponent<SquadColliderTracker>();
            selectCollider.Init(membership);

            SquadColliderTracker detectCollider = Instantiate(_dummyColliderPrefab, transform)
                .GetComponent<SquadColliderTracker>();
            detectCollider.Init(membership);
        }

        private void DeregisterMember(UnitDeathEvent evnt) {
            SquadMemberEntry deadEntry = _members[evnt.Entity.Id];
            
            _members.Remove(deadEntry.InstanceId);

            if (_members.Count > 0) return;
            
            _eventAgent.PostGlobal(new UnitDeathEvent(_entity));
            Destroy(gameObject, 0.1f);
        }

        public override void OnDestroy() {
            _members.Clear();
        }

        public SquadManager Get() => this;

        public string Key => "squad_manager";
    }
}