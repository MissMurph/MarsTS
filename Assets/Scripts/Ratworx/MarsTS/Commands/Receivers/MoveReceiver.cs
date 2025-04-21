using System;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class MoveReceiver : MonoBehaviour
    {
        private UnitPathfinder _unitPathing;
        private CommandQueue _commandQueue;
        private EventAgent _eventAgent;
        private MoveCommandlet _moveCommand;
        
        private void Awake() {
            _eventAgent = GetComponent<EventAgent>();
            _commandQueue = GetComponent<CommandQueue>();
            _unitPathing = GetComponent<UnitPathfinder>();
        }

        private void Start() {
            _eventAgent.AddListener<CommandStartEvent>(ReceiveCommand);
        }

        private void ReceiveCommand(CommandStartEvent evnt) {
            // TODO: Replace below with CommandKey match
            if (evnt.Command is not MoveCommandlet deserialized) 
                return;

            _moveCommand = deserialized;
            
            _unitPathing.FindPathTo(deserialized.Target);
            _eventAgent.AddListener<PathCompleteEvent>(OnPathComplete);
            _moveCommand.Callback.AddListener(OnCommandComplete);
        }

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            _eventAgent.RemoveListener<PathCompleteEvent>(OnPathComplete);
            evnt.Command.Callback.RemoveListener(OnCommandComplete);
            _moveCommand = null;
            _unitPathing.ClearPath();
        }

        private void OnPathComplete(PathCompleteEvent evnt) => _moveCommand.CompleteCommand(_commandQueue);
    }
}