using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.World;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Units
{
    public class HarvesterTurret : MonoBehaviour
    {
        //This is how many units per second
        [SerializeField] private int _harvestRate;

        private int _harvestAmount;

        private float _cooldown;
        private float _currentCooldown;

        private ResourceStorage _localStorage;

        [SerializeField] private GameObject _barrel;

        public float Range => _sensor.Range;

        private IHarvestable _target;

        private ISelectable _parent;
        private EventAgent _bus;

        private HarvestSensor _sensor;

        private void Awake()
        {
            _parent = GetComponentInParent<ISelectable>();
            _bus = GetComponentInParent<EventAgent>();
            _sensor = GetComponent<HarvestSensor>();

            _bus.AddListener<SensorUpdateEvent<IHarvestable>>(OnSensorUpdate);

            _localStorage = GetComponentInParent<ResourceStorage>();
            _localStorage.AttributeChangeEvent += OnStorageValueChange;

            _cooldown = 1f / _harvestRate;
            _harvestAmount = (int)(_harvestRate * _cooldown);
        }

        private void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

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

        private void FixedUpdate()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (_target != null && _sensor.IsDetected(_target))
                _barrel.transform.LookAt(_target.GameObject.transform, Vector3.up);
        }

        private void Harvest()
        {
            IHarvestable harvestable = _target;

            int harvested = harvestable.Harvest("resource_unit", _parent, _harvestAmount, _localStorage.Submit);
            
            _bus.Global(new ResourceHarvestedEvent(_bus, harvestable, _parent, ResourceHarvestedEvent.Side.Harvester,
                harvested, "resource_unit", _localStorage.Amount, _localStorage.Capacity));

            _currentCooldown += _cooldown;
        }

        private void OnStorageValueChange(int oldValue, int newValue)
        {
            _bus.Global(new ResourceHarvestedEvent(_bus, harvestable, _parent, ResourceHarvestedEvent.Side.Harvester,
                harvested, "resource_unit", _localStorage.Amount, _localStorage.Capacity));
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

        public bool IsInRange(IHarvestable target) => _sensor.IsDetected(target);
    }
}