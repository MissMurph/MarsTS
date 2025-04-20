using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class HarvestReceiver : MonoBehaviour,
                                   IEntityUpdate
    {
        [SerializeField] private HarvestSensor _harvestRange;
        [SerializeField] private DepositSensor _depositRange;
        [SerializeField] private ResourceStorage _storage;
        [SerializeField] private bool _automaticallyReturn;

        private HarvestableCommandlet _harvestCommand;
        private IDepositable _depositable;
        
        private UnitPathfinder _unitPathing;
        private UnitTargetManager _unitTargeting;
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
			
            _harvestCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<ResourceHarvestedEvent>(OnResourceHarvested);
            targetBus.AddListener<UnitDeathEvent>(OnDepositDepleted);
            _harvestCommand.Callback.AddListener(OnCommandComplete);
        }

        public void UpdateServer() {
            if (_depositable is not null) 
                UpdateDepositTarget();
            else 
                UpdateHarvestTarget();
        }

        private void UpdateHarvestTarget() {
            if (_harvestCommand is null) 
                return;

            if (_harvestRange.IsDetected(_harvestCommand.Target)) {
                // Do we really want to be clearing the target every single frame?
                _unitTargeting.ClearTarget();
                _unitPathing.ClearPath();
            }
            else
                _unitTargeting.SetTarget(_harvestCommand.Target);
        }

        private void UpdateDepositTarget() {
            if (_depositRange.IsDetected(_depositable)) {
                _unitTargeting.ClearTarget();
                _unitPathing.ClearPath();
            }
            else
                _unitTargeting.SetTarget(_depositable);
        }

        public void UpdateClient() { }
        
        private void Harvest(Commandlet order)
        {
            if (Stored >= Capacity) FindDepositable();

            if (order is Commandlet<IHarvestable> deserialized)
            {
                HarvestTarget = deserialized.Target;

                Bus.AddListener<ResourceHarvestedEvent>(OnResourceHarvested);

                EntityCache.TryGetEntityComponent(HarvestTarget.GameObject.transform.root.name, out EventAgent targetBus);

                targetBus.AddListener<UnitDeathEvent>(OnDepositDepleted);

                order.Callback.AddListener(OnCommandComplete);
            }
        }
        
        private void OnResourceHarvested(ResourceHarvestedEvent _event)
        {
            if (Stored >= Port.Capacity)
                //bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);
                //EntityCache.TryGet(_event.Deposit.GameObject.transform.root.name, out EventAgent targetBus);
                //targetBus.RemoveListener<EntityDeathEvent>(OnDepositDepleted);
                //CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);
                //CurrentCommand.Callback.Invoke(newEvent);
                FindDepositable();
        }

        private void OnDepositDepleted(UnitDeathEvent _event)
        {
            Bus.RemoveListener<ResourceHarvestedEvent>(OnResourceHarvested);

            CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

            CurrentCommand.Callback.Invoke(newEvent);
        }

        private void OnCommandComplete(CommandCompleteEvent _event)
        {
            if (_event.Command is Commandlet<IHarvestable> deserialized && _event.IsCancelled)
            {
                Bus.RemoveListener<ResourceHarvestedEvent>(OnResourceHarvested);

                EntityCache.TryGetEntityComponent(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

                targetBus.RemoveListener<UnitDeathEvent>(OnDepositDepleted);

                HarvestTarget = null;
                DepositTarget = null;
            }
        }
    }
}