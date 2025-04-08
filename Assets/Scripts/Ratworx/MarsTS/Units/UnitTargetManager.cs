using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Pathfinding;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class UnitTargetManager : MonoBehaviour, IEntityComponent<UnitTargetManager>
    {
        public Action<Transform> OnTargetChanged;
        public Vector3 Position => TargetTransform?.position ?? _targetPosition;
        private Vector3 _targetPosition;
        public bool IsTransform => TargetTransform;

        public Transform TargetTransform
        {
            get => _targetTransform;
            set
            {
                if (_targetTransform != null)
                {
                    EntityCache.TryGetEntityComponent(_targetTransform.gameObject.name + ":eventAgent", out EventAgent oldAgent);
                    oldAgent.RemoveListener<UnitDeathEvent>(OnTargetDeath);
                }

                _targetTransform = value;

                if (value != null)
                {
                    EntityCache.TryGetEntityComponent(value.gameObject.name + ":eventAgent", out EventAgent agent);
                    agent.AddListener<UnitDeathEvent>(OnTargetDeath);
                }
            }
        }

        public string Key => "target";
        public UnitTargetManager Get() => this;
        
        private Transform _targetTransform;

        public void SetTarget(Transform target)
        {
            _targetTransform = target;
            OnTargetChanged?.Invoke(target);
        }

        private void OnTargetDeath(UnitDeathEvent _event) {
            TargetTransform = null;
        }
    }
}