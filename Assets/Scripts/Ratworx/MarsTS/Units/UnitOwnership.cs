using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;

namespace Ratworx.MarsTS.Units
{
    public class UnitOwnership : NetworkBehaviour,
                                 IEntityComponent<UnitOwnership>
    {
        public Faction Owner { get; private set; }

        public Action<Faction> OnUnitOwnershipChanged;
        
        private EventAgent _eventAgent;

        public UnitOwnership Get() => this;
        public string Key => "ownership";
        
        public bool SetOwner(Faction faction)
        {
            if (!NetworkManager.Singleton.IsServer) return false;

            Owner = faction;
            SetOwnerClientRpc(Owner.Id);
            // _eventAgent.Global(new UnitOwnerChangeEvent(_eventAgent, this, Owner));
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId)
        {
            Owner = TeamCache.Faction(newId);
            // _eventAgent.Global(new UnitOwnerChangeEvent(_eventAgent, this, Owner));
        }
        
        public Relationship GetRelationship(Faction other) => Owner.GetRelationship(other);
    }
}