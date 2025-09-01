using System;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Units.Sensors;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class Depositor : MonoBehaviour,
                             IEntityServerUpdate
    {
        [SerializeField] private ResourceStorage _storage;
        [SerializeField] private DepositSensor _sensor;
        //This is how many units per second
        [SerializeField] private float _depositRate;

        private UnitTargetManager _unitTargeting;
        private EventAgent _eventAgent;
        private Entity _entity;
        private IDepositable _trackedTarget;
        
        [SerializeField] private int _depositAmount;
        [SerializeField] private float _cooldown;
        private float _currentCooldown;

        private void Awake() {
            _unitTargeting = GetComponentInParent<UnitTargetManager>();
            _eventAgent = GetComponentInParent<EventAgent>();
            _entity = GetComponentInParent<Entity>();
            
            // _cooldown = 1f / _depositRate;
            // _depositAmount = Mathf.RoundToInt(_depositRate * _cooldown);
            _currentCooldown = _cooldown;
        }
        
        private void Start() {
            _sensor.OnUnitDetected += OnUnitDetected;
        }

        private void OnDestroy() {
            _sensor.OnUnitDetected -= OnUnitDetected;
        }

        private void OnDisable() {
            _trackedTarget = null;
        }
        
        // TODO: Investigate linking receiver so this only activates when intending so
        private void OnUnitDetected(IDepositable unit, bool detected) {
            if (!detected && _trackedTarget == unit) {
                _trackedTarget = GetClosestDetected();
                return;
            }

            if (_unitTargeting.TargetUnit is IDepositable && unit == _unitTargeting.TargetUnit) {
                _trackedTarget = unit;
                return;
            }
            
            if (_trackedTarget != null) return;

            if (detected) 
                _trackedTarget = unit;
        }
        
        public void UpdateServer() {
            if (_storage.Value <= 0) return;
            
            if (_currentCooldown > 0f) 
                _currentCooldown -= Time.deltaTime;
            
            if (_trackedTarget == null) 
                return;

            if (_currentCooldown > 0f)
                return;
            
            DepositResources(_trackedTarget);
            _currentCooldown += _cooldown;
        }

        // TODO: Investigate if we actually need this after reworking Roughnecks logic
        private void DepositResources(IDepositable depositTarget) {
            int depositAmount = Math.Min(_storage.Value, _depositAmount);
            
            _storage.Value -= depositTarget.Deposit(_storage.Resource, depositAmount);
            
            _eventAgent.PostGlobal(new HarvesterDepositEvent(
                _entity,
                HarvesterDepositEvent.Side.Harvester,
                _storage.Value,
                _storage.Capacity,
                depositTarget)
            );
            
            _currentCooldown += _cooldown;
        }
        
        // TODO: Make function in sensor
        private IDepositable GetClosestDetected() {
            float distance = _sensor.Range * _sensor.Range;
            IDepositable currentClosest = null;

            foreach (IDepositable unit in _sensor.Detected) {
                float newDistance =
                    Vector3.Distance(_sensor.GetDetectedCollider(unit.GameObject.name).transform.position, 
                        transform.position);

                if (newDistance < distance) currentClosest = unit;
            }

            return currentClosest;
        }
    }
}