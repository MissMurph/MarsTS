using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Sensors;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class AttackReceiver : MonoBehaviour,
                                  IEntityUpdate
    {
        [SerializeField] private AttackableSensor _targetTrackRange;
        
        private UnitPathfinder _unitPathing;
        private UnitTargetManager _unitTargeting;
        private CommandQueue _commandQueue;
        private EventAgent _eventAgent;
        private AttackableCommandlet _attackCommand;
        
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
            if (evnt.Command is not AttackableCommandlet deserialized
                || evnt.Command.Name != "attack") 
                return;

            _attackCommand = deserialized;
            _unitTargeting.SetTarget(_attackCommand.Target);

            _attackCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);
            _attackCommand.Callback.AddListener(OnCommandComplete);
        }

        public void UpdateServer() {
            if (_attackCommand is null) 
                return;

            if (_targetTrackRange.IsDetected(_attackCommand.Target)) {
                _unitTargeting.ClearTarget();
                _unitPathing.ClearPath();
            }
            else
                _unitTargeting.SetTarget(_attackCommand.Target);
        }

        // stimky...
        public void UpdateClient() { }

        private void OnTargetDeath (UnitDeathEvent evnt) => _attackCommand.CompleteCommand(_commandQueue);

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            _attackCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            
            targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);
            evnt.Command.Callback.RemoveListener(OnCommandComplete);
            
            _attackCommand = null;
            
            _unitTargeting.ClearTarget();
            _unitPathing.ClearPath();
        }
        
        /*public override CommandFactory Evaluate (ISelectable target) {
            if (target is IAttackable && target.GetRelationship(Owner) == Relationship.Hostile) {
                return CommandPrimer.Get("attack");
            }

            return CommandPrimer.Get("move");
        }

        public override void AutoCommand (ISelectable target) {
            if (target is IAttackable deserialized && target.GetRelationship(Owner) == Relationship.Hostile) {
                CommandPrimer.Get<Attack>("attack").Construct(deserialized);
            }

            CommandPrimer.Get<Move>("move").Construct(target.GameObject.transform.position);
        }*/
    }
}