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
    [RequireComponent(typeof(UnitOwnership))]
    public class UnitSelection : MonoBehaviour,
                                 ISelectable,
                                 IEntityComponent<UnitSelection>
    {
        public Action<bool> OnUnitSelectionChange;
        public Action<bool> OnUnitHoverChange;
        public int Id => Entity.Id;
        public string UnitType => Entity.RegistryKey;
        public string RegistryKey => $"{Entity.RegistryType}:{Entity.RegistryKey}";
        public Faction Owner => _unitOwnership.Owner;
        public Sprite Icon => _icon;
        public GameObject GameObject => gameObject;
        public Entity Entity { get; private set; }

        [SerializeField] private Sprite _icon;

        private EventAgent _eventAgent;
        private UnitOwnership _unitOwnership;

        private void Awake() {
            Entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();
            _unitOwnership = GetComponent<UnitOwnership>();
        }

        public void Select(bool status) {
            OnUnitSelectionChange?.Invoke(status);
            _eventAgent.PostLocal(new UnitSelectEvent(_eventAgent, status));
        }

        public void Hover(bool status) {
            OnUnitHoverChange?.Invoke(status);
            _eventAgent.PostLocal(new UnitHoverEvent(_eventAgent, status));
        }

        public Relationship GetRelationship(Faction other) => _unitOwnership.GetRelationship(other);
        public UnitSelection Get() => this;

        public string Key => "selectable";
    }
}