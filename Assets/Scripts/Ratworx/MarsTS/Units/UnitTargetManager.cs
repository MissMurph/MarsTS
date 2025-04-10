using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class UnitTargetManager : MonoBehaviour, IEntityComponent<UnitTargetManager>
    {
        public Action<IUnitInterface> OnTargetChanged;
        public IUnitInterface TargetUnit => _unit;
        public Transform TargetTransform => _unit.GameObject.transform;
        public string Key => "target";
        public UnitTargetManager Get() => this;
        
        private IUnitInterface _unit;
        
        public void SetTarget(IUnitInterface unit)
        {
            if (_unit != null) {
                _unit.Entity.TryGetEntityComponent("eventAgent", out EventAgent oldAgent);
                oldAgent.RemoveListener<UnitDeathEvent>(OnEntityDeath);
                oldAgent.RemoveListener<EntityVisibleEvent>(OnEntityVisible);
            }

            _unit = unit;

            if (_unit != null) {
                _unit.Entity.TryGetEntityComponent("eventAgent", out EventAgent agent);
                agent.AddListener<UnitDeathEvent>(OnEntityDeath);
                agent.AddListener<EntityVisibleEvent>(OnEntityVisible);
            }
            
            OnTargetChanged?.Invoke(_unit);
        }
        
        private void OnEntityDeath(UnitDeathEvent _event) => SetTarget(null);

        private void OnEntityVisible(EntityVisibleEvent _event) {
            if (!_event.Visible) SetTarget(null);
        }
    }
}