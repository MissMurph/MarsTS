using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings.Ghosts
{
    [RequireComponent(typeof(UnitOwnership))]
    public class ConstructionGhostSelection : MonoBehaviour,
                                              ISelectable,
                                              IEntityComponent<ConstructionGhostSelection>
    {
        public Action<bool> OnUnitSelectionChange;
        public Action<bool> OnUnitHoverChange;
        public int Id => Entity.Id;
        public string UnitType => _buildingEntity.RegistryKey;
        public string RegistryKey => $"{_buildingEntity.RegistryType}:{_buildingEntity.RegistryKey}";
        public Faction Owner => _unitOwnership.Owner;
        public Sprite Icon => _buildingSelection.Icon;
        public GameObject GameObject => gameObject;
        public Entity Entity { get; private set; }
        public bool IsSelected => _selected;
        
        private EventAgent _eventAgent;
        private UnitOwnership _unitOwnership;

        private Entity _buildingEntity;
        private ISelectable _buildingSelection;
        [SerializeField] private bool _selected;

        private void Awake() {
            Entity = GetComponent<Entity>();
            _eventAgent = GetComponent<EventAgent>();
            _unitOwnership = GetComponent<UnitOwnership>();
        }

        public void SetBuildingOverride(string registryKey) {
            if (!Registry.Registry.TryGetPrefab(registryKey, out GameObject prefab)) {
                RatLogger.Error?.Log($"Couldn't find prefab {registryKey} for construction ghost! Destroying ghost.");
                Destroy(gameObject, 0.1f);
                return;
            }

            _buildingEntity = prefab.GetComponent<Entity>();
            _buildingSelection = prefab.GetComponent<ISelectable>();
        }

        public void Select(bool status) {
            _selected = status;
            OnUnitSelectionChange?.Invoke(status);
            _eventAgent.PostLocal(new UnitSelectEvent(status));
        }

        public void Hover(bool status) {
            OnUnitHoverChange?.Invoke(status);
            _eventAgent.PostLocal(new UnitHoverEvent(status));
        }

        public Relationship GetRelationship(Faction other) => _unitOwnership.GetRelationship(other);
        public ConstructionGhostSelection Get() => this;
        public string Key => "selectable";
    }
}