using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    [RequireComponent(typeof(UnitSelection))]
    public class UnitOwnership : NetworkBehaviour,
                                 IEntityComponent<UnitOwnership>,
                                 IUnitInterface
    {
        public Faction Owner { get; private set; }
        public Action<Faction> OnUnitOwnershipChanged;
        public UnitOwnership Get() => this;
        public string Key => "ownership";
        public GameObject GameObject => gameObject;
        public Entity Entity => _entity;
        
        private EventAgent _eventAgent;
        private Entity _entity;

        private void Awake() {
            _entity = GetComponent<Entity>();
        }

        public bool SetOwner(Faction faction)
        {
            if (!NetworkManager.Singleton.IsServer) return false;

            Owner = faction;
            SetOwnerClientRpc(Owner.Id);
            _eventAgent.PostGlobal(new UnitOwnerChangeEvent(this, Owner));
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId)
        {
            Owner = TeamCache.Faction(newId);
            _eventAgent.PostGlobal(new UnitOwnerChangeEvent(this, Owner));
        }

        public Relationship GetRelationship(Faction other) => Owner.GetRelationship(other);
    }
}