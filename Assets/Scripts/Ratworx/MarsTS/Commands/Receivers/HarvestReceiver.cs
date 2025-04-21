using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class HarvestReceiver : MonoBehaviour
    {
        [SerializeField] private HarvestSensor _harvestRange;
        [SerializeField] private DepositSensor _depositRange;
        [SerializeField] private ResourceStorage _storage;
        [SerializeField] private bool _automaticallyReturn;

        private HarvestableCommandlet _harvestCommand;
        private IDepositable _depositable;
        
        private UnitPathfinder _unitPathing;
        private UnitTargetManager _unitTargeting;
        private UnitOwnership _ownership;
        private CommandQueue _commandQueue;
        private EventAgent _eventAgent;

        private void Awake() {
            _eventAgent = GetComponent<EventAgent>();
            _commandQueue = GetComponent<CommandQueue>();
            _unitPathing = GetComponent<UnitPathfinder>();
            _unitTargeting = GetComponent<UnitTargetManager>();
        }
        
        private void Start() {
            _eventAgent.AddListener<CommandStartEvent>(ReceiveCommand);
        }

        private void ReceiveCommand(CommandStartEvent evnt) {
            if (evnt.Command is not HarvestableCommandlet deserialized
                || evnt.Command.Name != "harvest") return;
            
            _harvestCommand = deserialized;
            _unitTargeting.SetTarget(_harvestCommand.Target);
            
            _storage.OnAttributeChange += OnResourceHarvested;
            _storage.OnAttributeChange += OnResourceDeposited;
            _harvestRange.OnUnitDetected += OnHarvestableDetected;
            _depositRange.OnUnitDetected += OnDepositableDetected;
			
            _harvestCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<UnitDeathEvent>(OnDepositDepleted);
            _harvestCommand.Callback.AddListener(OnCommandComplete);
        }

        private void OnHarvestableDetected(IHarvestable unit, bool detected) {
            if (!detected
                || unit.Entity != _harvestCommand.Target.Entity) 
                return;
            
            _unitPathing.ClearPath();
            _unitTargeting.ClearTarget();
        }

        private void OnDepositableDetected(IDepositable unit, bool detected) {
            if (_depositable is null
                || !detected
                || unit.Entity != _depositable.Entity) 
                return;
            
            _unitPathing.ClearPath();
            _unitTargeting.ClearTarget();
        }

        private void OnResourceHarvested(int oldValue, int newValue) {
            if (newValue < oldValue
                || newValue < _storage.Capacity)
                return;
            
            FindDepositable();
        }

        private void OnResourceDeposited(int oldValue, int newValue) {
            if (newValue > oldValue
                || newValue > 0)
                return;
            
            _unitTargeting.SetTarget(_harvestCommand.Target);
            _depositable = null;
        }
        
        private void FindDepositable() {
            IDepositable closestBank = null;
            const float currentDist = 1000f;

            foreach (IDepositable bank in _ownership.Owner.GetOwnedDepositables()) {
                float newDistance = Vector3.Distance(bank.GameObject.transform.position, transform.position);
                if (newDistance < currentDist) closestBank = bank;
            }

            if (closestBank != null) 
                _depositable = closestBank;
            else
                _harvestCommand?.CompleteCommand(_commandQueue, true);
        }

        private void OnDepositDepleted(UnitDeathEvent evnt) => _harvestCommand.CompleteCommand(_commandQueue);

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            _storage.OnAttributeChange -= OnResourceHarvested;
            _storage.OnAttributeChange -= OnResourceDeposited;
            _harvestRange.OnUnitDetected -= OnHarvestableDetected;
            _depositRange.OnUnitDetected -= OnDepositableDetected;
            
            _harvestCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.RemoveListener<UnitDeathEvent>(OnDepositDepleted);
            _harvestCommand.Callback.RemoveListener(OnCommandComplete);
            _harvestCommand = null;
        }
    }
}