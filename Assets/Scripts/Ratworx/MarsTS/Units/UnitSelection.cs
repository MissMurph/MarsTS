using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class UnitSelection : NetworkBehaviour,
                                 ISelectable,
                                 IEntityComponent<UnitSelection>
    {
        public Action<bool> OnUnitSelectionChange;
        public Action<bool> OnUnitHoverChange;
        public int Id => Entity.Id;
        public string UnitType => Entity.RegistryKey;
        public string RegistryKey => $"{Entity.RegistryType}:{Entity.RegistryKey}";
        public Sprite Icon => _icon;
        public Entity Entity { get; private set; }

        [SerializeField]
        private Sprite _icon;

        private EventAgent _eventAgent;
        private UnitOwnership _unitOwnership;

        private void Awake() {
            Entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();
            _unitOwnership = GetComponent<UnitOwnership>();
        }
        
        public virtual void Select(bool status)
        {
            OnUnitSelectionChange?.Invoke(status);
            _eventAgent.Local(new UnitSelectEvent(_eventAgent, status));
        }

        public virtual void Hover(bool status)
        {
            OnUnitHoverChange?.Invoke(status);
            _eventAgent.Local(new UnitHoverEvent(_eventAgent, status));
        }
        
        public Relationship GetRelationship(Faction other) => _unitOwnership.GetRelationship(other);
    }
}