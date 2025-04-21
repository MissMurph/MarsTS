using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class UnitTargetManager : MonoBehaviour, 
                                     IEntityComponent<UnitTargetManager>
    {
        /// <remarks>The unit will be null on a clear</remarks>
        public Action<IUnitInterface> OnTargetChanged;
        public IUnitInterface TargetUnit => _unit;
        public Transform TargetTransform => _unit.GameObject.transform;
        public string Key => "target";
        public UnitTargetManager Get() => this;
        
        private IUnitInterface _unit;

        public void SetTarget(IUnitInterface unit) {
            if (_unit == null)
                return;

            _unit.Entity.TryGetEntityComponent("eventAgent", out EventAgent agent);
            agent.AddListener<UnitDeathEvent>(OnEntityDeath);
            agent.AddListener<EntityVisibleEvent>(OnEntityVisible);

            _unit = unit;
            OnTargetChanged?.Invoke(_unit);
        }

        public void ClearTarget() {
            if (_unit == null) 
                return;
            
            _unit.Entity.TryGetEntityComponent("eventAgent", out EventAgent oldAgent);
            oldAgent.RemoveListener<UnitDeathEvent>(OnEntityDeath);
            oldAgent.RemoveListener<EntityVisibleEvent>(OnEntityVisible);

            _unit = null;
            OnTargetChanged?.Invoke(_unit);
        }
        
        private void OnEntityDeath(UnitDeathEvent _event) => SetTarget(null);

        private void OnEntityVisible(EntityVisibleEvent _event) {
            if (!_event.Visible) ClearTarget();
        }
    }
}