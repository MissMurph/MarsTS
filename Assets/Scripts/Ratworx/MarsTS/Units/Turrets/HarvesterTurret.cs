using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.WorldObject;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class HarvesterTurret : MonoBehaviour,
                                   IEntityServerUpdate
    {
        //This is how many units per second
        [SerializeField] private int _harvestRate;

        private int _harvestAmount;
        private float _cooldown;
        private float _currentCooldown;

        private ResourceStorage _localStorage;

        // [SerializeField] private GameObject _barrel;
        
        private UnitTargetManager _unitTargeting;
        private EventAgent _bus;

        [SerializeField] private HarvestSensor _sensor;

        private void Awake()
        {
            _bus = GetComponentInParent<EventAgent>();

            _bus.AddListener<SensorUpdateEvent<IHarvestable>>(OnSensorUpdate);

            _localStorage = GetComponentInParent<ResourceStorage>();
            _localStorage.OnAttributeChange += OnStorageValueChange;

            _cooldown = 1f / _harvestRate;
            _harvestAmount = (int)(_harvestRate * _cooldown);
        }

        public void UpdateServer()
        {
            if (_currentCooldown >= 0f) _currentCooldown -= Time.deltaTime;

            if (_parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null &&
                commandableUnit.CurrentCommand.Name == "harvest")
            {
                var harvestCommand = commandableUnit.CurrentCommand as Commandlet<IHarvestable>;

                if (_sensor.IsDetected(harvestCommand.Target)) _target = harvestCommand.Target;
            }

            if (_target == null)
                foreach (IHarvestable unit in _sensor.Detected)
                {
                    _target = unit;
                    break;
                }

            if (_target != null && _sensor.IsDetected(_target) && _currentCooldown <= 0) Harvest();
        }

        private void Harvest()
        {
            IHarvestable harvestable = _target;

            int harvested = harvestable.Harvest(_localStorage.Resource, _parent, _harvestAmount, _localStorage.Submit);
            
            _bus.PostGlobal(new ResourceHarvestedEvent(_bus, _parent, ResourceHarvestedEvent.Side.Harvester,
                harvested, _localStorage.Resource, _localStorage.Value, _localStorage.Capacity));

            _currentCooldown += _cooldown;
        }

        private void OnStorageValueChange(int oldValue, int newValue)
        {
            _bus.PostGlobal(new ResourceHarvestedEvent(_bus, _parent, ResourceHarvestedEvent.Side.Harvester,
                newValue - oldValue, _localStorage.Resource, _localStorage.Value, _localStorage.Capacity));
        }

        private void OnSensorUpdate(SensorUpdateEvent<IHarvestable> _event)
        {
            if (_event.Detected)
            {
                if (_target == null) _target = _event.Target;
            }
            else if (ReferenceEquals(_event.Target, _target))
            {
                _target = null;
            }
        }
    }
}