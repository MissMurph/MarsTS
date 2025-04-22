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
        
        private int _depositAmount;
        private float _cooldown;
        private float _currentCooldown;

        private void Awake() {
            _unitTargeting = GetComponent<UnitTargetManager>();
            _eventAgent = GetComponent<EventAgent>();
            _entity = GetComponent<Entity>();
            
            _cooldown = 1f / _depositRate;
            _depositAmount = Mathf.RoundToInt(_depositRate * _cooldown);
            _currentCooldown = _cooldown;
        }
        
        public void UpdateServer() {
            if (_unitTargeting.TargetUnit is not IDepositable depositable
                || !_sensor.IsDetected(depositable)) 
                return;

            _currentCooldown -= Time.deltaTime;
            if (_currentCooldown <= 0f) DepositResources(depositable);
        }

        // TODO: Investigate if we actually need this after reworking Roughnecks logic
        private void DepositResources(IDepositable depositTarget) {
            _storage.Value -= depositTarget.Deposit(_storage.Resource, _depositAmount);
            
            _eventAgent.PostGlobal(new HarvesterDepositEvent(
                _entity,
                HarvesterDepositEvent.Side.Harvester,
                _storage.Value,
                _storage.Capacity,
                depositTarget)
            );
            
            _currentCooldown += _cooldown;
        }
    }
}