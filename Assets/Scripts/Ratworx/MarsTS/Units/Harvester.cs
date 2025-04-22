using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class Harvester : MonoBehaviour, 
                             IEntityServerUpdate
    {
        [SerializeField] private HarvestSensor _sensor;
        //This is how many units per second
        [SerializeField] private int _harvestRate;

        private int _harvestAmount;
        private float _cooldown;
        private float _currentCooldown;

        private ResourceStorage _localStorage;
        private UnitTargetManager _unitTargeting;
        private EventAgent _eventAgent;
        private Entity _entity;

        private void Awake() {
            _eventAgent = GetComponentInParent<EventAgent>();
            _entity = GetComponentInParent<Entity>();

            _localStorage = GetComponentInParent<ResourceStorage>();
            _localStorage.OnAttributeChange += OnStorageValueChange;

            _cooldown = 1f / _harvestRate;
            _harvestAmount = (int)(_harvestRate * _cooldown);
        }

        public void UpdateServer() {
            if (_unitTargeting.TargetUnit is not IHarvestable harvestable
                || !_sensor.IsDetected(harvestable)) 
                return;
            
            _currentCooldown -= Time.deltaTime;
            if (_currentCooldown <= 0) Harvest(harvestable);
        }

        private void Harvest(IHarvestable harvestable)
        {
            int harvested = harvestable.Harvest(_localStorage.Resource, _entity, _harvestAmount, _localStorage.Submit);
            
            _eventAgent.PostGlobal(new ResourceHarvestedEvent(_entity, ResourceHarvestedEvent.Side.Harvester,
                harvested, _localStorage.Resource, _localStorage.Value, _localStorage.Capacity));

            _currentCooldown += _cooldown;
        }

        // TODO: Investigate if this can be handled via UI <-> Attribute
        private void OnStorageValueChange(int oldValue, int newValue)
        {
            _eventAgent.PostGlobal(new ResourceHarvestedEvent(_entity, ResourceHarvestedEvent.Side.Harvester,
                newValue - oldValue, _localStorage.Resource, _localStorage.Value, _localStorage.Capacity));
        }
    }
}