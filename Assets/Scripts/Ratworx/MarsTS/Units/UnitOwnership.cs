using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class UnitOwnership : NetworkBehaviour,
                                 IEntityComponent<UnitOwnership>,
                                 IUnitInterface
    {
        public Action<Faction> OnUnitOwnershipChanged;

        public Faction Owner {
            get => _owner;
            private set => _owner = value;
        }

        public UnitOwnership Get() => this;
        public string Key => "ownership";
        public GameObject GameObject => gameObject;
        public Entity Entity { get; private set; }

        private EventAgent _eventAgent;
        [SerializeField] private Faction _owner;

        private void Awake() {
            Entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();
        }

        public bool SetOwner(Faction faction) {
            if (!NetworkManager.Singleton.IsServer) return false;

            Owner = faction;
            SetOwnerClientRpc(Owner.Id);
            _eventAgent.PostGlobal(new UnitOwnerChangeEvent(this, Owner));
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId) {
            Owner = TeamCache.Faction(newId);
            _eventAgent.PostGlobal(new UnitOwnerChangeEvent(this, Owner));
        }

        public Relationship GetRelationship(Faction other) => Owner.GetRelationship(other);
    }
}