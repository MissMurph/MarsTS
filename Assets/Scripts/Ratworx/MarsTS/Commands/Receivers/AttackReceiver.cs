using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.UI;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Sensors;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class AttackReceiver : AbstractCommandReceiver<AttackableCommandlet>
    {
        public override bool CanCommand => true;
        public override bool IsActive => false;
        public override float Cooldown => 0f;
        
        [SerializeField] private AttackableSensor _targetTrackRange;

        private AttackableCommandlet _attackCommand;

        public override void ReceiveCommand(AttackableCommandlet command) {
            _attackCommand = command;
            UnitTargeting.SetTarget(_attackCommand.Target);
            
            _targetTrackRange.OnUnitDetected += OnTargetDetected;
            
            _attackCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);
            _attackCommand.OnCommandComplete.AddListener(OnCommandComplete);
        }

        public override (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) {
            if (!entity.TryGetEntityComponent(out IAttackable attackable)
                || attackable.GetRelationship(Ownership.Owner) != Relationship.Hostile) 
                return (false, null);
            
            return (true, CommandPrimer.GetInterface(CommandKey));
        }

        private void OnTargetDetected(IAttackable unit, bool detected) {
            if (unit.Entity != _attackCommand.Target.Entity) return;

            if (detected) {
                UnitPathing.ClearPath();
                UnitTargeting.ClearTarget();
            }
            else
                UnitTargeting.SetTarget(unit);
        }

        private void OnTargetDeath (UnitDeathEvent evnt) => _attackCommand.CompleteCommand(CommandQueue);

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            _attackCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            
            targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);
            evnt.Command.OnCommandComplete.RemoveListener(OnCommandComplete);
            
            _targetTrackRange.OnUnitDetected -= OnTargetDetected;
            _attackCommand = null;
            
            UnitTargeting.ClearTarget();
            UnitPathing.ClearPath();
        }
    }
}