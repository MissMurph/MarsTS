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
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class HarvestReceiver : MonoBehaviour,
                                   IEntityUpdate
    {
        [SerializeField] private HarvestSensor _harvestRange;

        private UnitPathfinder _unitPathing;
        private UnitTargetManager _unitTargeting;
        private CommandQueue _commandQueue;
        private EventAgent _eventAgent;
        
        private HarvestableCommandlet _harvestCommand;

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
                || evnt.Command.Name != "harvest") 
                return;
			
            IHarvestable unit = deserialized.Target;
			
            _harvestCommand = deserialized;
            _unitTargeting.SetTarget(_harvestCommand.Target);
			
            _harvestCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<UnitHurtEvent>(OnTargetHealed);
            targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);
            _harvestCommand.Callback.AddListener(OnCommandComplete);
        }
        
        public void UpdateServer() {
            if (_harvestCommand is null) 
                return;
			
            if (_harvestRange.IsDetected(_harvestCommand.Target)) {
                _unitTargeting.ClearTarget();
                _unitPathing.ClearPath();
            }
            else
                _unitTargeting.SetTarget(_repairCommand.Target);
        }

        public void UpdateClient() { }
        
        private void Harvest(Commandlet order)
        {
            if (Stored >= Port.Capacity) FindDepositable();

            if (order is Commandlet<IHarvestable> deserialized)
            {
                HarvestTarget = deserialized.Target;

                Bus.AddListener<ResourceHarvestedEvent>(OnExtraction);

                EntityCache.TryGetEntityComponent(HarvestTarget.GameObject.transform.root.name, out EventAgent targetBus);

                targetBus.AddListener<UnitDeathEvent>(OnDepositDepleted);

                order.Callback.AddListener(HarvestCancelled);
            }
        }
        
        private void OnExtraction(ResourceHarvestedEvent _event)
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
            Bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

            CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

            CurrentCommand.Callback.Invoke(newEvent);
        }

        private void HarvestCancelled(CommandCompleteEvent _event)
        {
            if (_event.Command is Commandlet<IHarvestable> deserialized && _event.IsCancelled)
            {
                Bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

                EntityCache.TryGetEntityComponent(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

                targetBus.RemoveListener<UnitDeathEvent>(OnDepositDepleted);

                HarvestTarget = null;
                DepositTarget = null;
            }
        }
    }
}