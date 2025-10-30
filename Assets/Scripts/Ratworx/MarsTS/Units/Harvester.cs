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
        private IHarvestable _trackedTarget;

        private ResourceStorage _localStorage;
        private UnitTargetManager _unitTargeting;
        private EventAgent _eventAgent;
        private Entity _entity;

        private void Awake() {
            _eventAgent = GetComponentInParent<EventAgent>();
            _entity = GetComponentInParent<Entity>();
            _unitTargeting = GetComponentInParent<UnitTargetManager>();

            _localStorage = GetComponentInParent<ResourceStorage>();
            _localStorage.OnAttributeChange += OnStorageValueChange;

            _cooldown = 1f / _harvestRate;
            _harvestAmount = (int)(_harvestRate * _cooldown);
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
        private void OnUnitDetected(IHarvestable unit, bool detected) {
            if (unit is null
                || !unit.CanHarvest(_localStorage.Resource, _entity))
                return;
            
            // Switch units if losing detection of current target
            if (!detected && _trackedTarget == unit) {
                _trackedTarget = GetClosestDetected();
                return;
            }

            // Prioritize what unitTargeting (from commands) is targeting
            if (_unitTargeting?.TargetUnit is IHarvestable 
                && unit == _unitTargeting?.TargetUnit
                && unit.CanHarvest(_localStorage.Resource, _entity)) {
                _trackedTarget = unit;
                return;
            }
            
            if (_trackedTarget != null) return;

            if (detected) 
                _trackedTarget = unit;
        }

        public void UpdateServer() {
            if (_currentCooldown > 0f) 
                _currentCooldown -= Time.deltaTime;
            
            if (_trackedTarget == null) 
                return;

            if (_currentCooldown > 0f)
                return;
            
            Harvest(_trackedTarget);
            _currentCooldown += _cooldown;
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
        
        // TODO: Make function in sensor
        private IHarvestable GetClosestDetected() {
            float distance = _sensor.Range * _sensor.Range;
            IHarvestable currentClosest = null;

            foreach (IHarvestable unit in _sensor.Detected) {
                float newDistance =
                    Vector3.Distance(_sensor.GetDetectedCollider(unit.GameObject.name).transform.position, 
                        transform.position);

                if (newDistance < distance) currentClosest = unit;
            }

            return currentClosest;
        }
    }
}