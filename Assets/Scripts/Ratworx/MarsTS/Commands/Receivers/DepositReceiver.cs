using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Sensors;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class DepositReceiver : MonoBehaviour,
                                   IEntityUpdate
    {
        [SerializeField] private DepositSensor _depositRange;
        [SerializeField] private ResourceStorage _storage;
        
        private DepositableCommandlet _depositCommand;

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
            if (evnt.Command is not DepositableCommandlet deserialized
                || evnt.Command.Name != "harvest") return;
            
            _depositCommand = deserialized;
            _unitTargeting.SetTarget(_depositCommand.Target);

            _depositCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<HarvesterDepositEvent>(OnResourceDeposited);
            targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);
            _depositCommand.Callback.AddListener(OnC);
        }
    }
}