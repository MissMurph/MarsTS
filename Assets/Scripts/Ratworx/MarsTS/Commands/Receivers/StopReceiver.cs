using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class StopReceiver : MonoBehaviour
    {
        private UnitPathfinder _unitPathing;
        private UnitTargetManager _unitTargeting;
        private CommandQueue _commandQueue;
        private EventAgent _eventAgent;
        
        private void Awake() {
            _eventAgent = GetComponent<EventAgent>();
            _commandQueue = GetComponent<CommandQueue>();
            _unitPathing = GetComponent<UnitPathfinder>();
        }

        private void Start() {
            _eventAgent.AddListener<CommandStartEvent>(ReceiveCommand);
        }

        private void ReceiveCommand(CommandStartEvent evnt) {
            if (evnt.Command is not SimpleCommandlet
                || evnt.Command.name != "stop")
                return;

            _unitPathing.ClearPath();
            _unitTargeting.ClearTarget();
            _commandQueue.Clear();
            evnt.Command.CompleteCommand(_commandQueue);
        }
    }
}