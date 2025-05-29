using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands.Commandlets;
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
    public class DepositReceiver : AbstractCommandReceiver<DepositableCommandlet>
    {
        public override bool CanCommand => true;
        public override bool IsActive => false;
        public override float Cooldown => 0f;
        
        [SerializeField] private DepositSensor _depositRange;
        [SerializeField] private ResourceStorage _storage;
        
        private DepositableCommandlet _depositCommand;
        
        public override void ReceiveCommand(DepositableCommandlet command) {
            _depositCommand = command;
            UnitTargeting.SetTarget(_depositCommand.Target);

            _depositRange.OnUnitDetected += OnDepositableDetected;
            _storage.OnAttributeChange += OnResourceDeposited;

            _depositCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);
            _depositCommand.OnCommandComplete.AddListener(OnCommandComplete);
        }

        public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity) {
            if (!entity.TryGetEntityComponent(out IDepositable _)
                || !entity.TryGetEntityComponent(out UnitOwnership targetOwnership)
                || targetOwnership.GetRelationship(Ownership.Owner) != Relationship.Owned
                || _storage.Value <= 0)
                return (false, null);

            return (true, CommandPrimer.Get(CommandKey));
        }

        private void OnDepositableDetected(IDepositable unit, bool detected) {
            if (unit.Entity != _depositCommand.Target.Entity) return;

            if (detected) {
                UnitPathing.ClearPath();
                UnitTargeting.ClearTarget();
            }
            else
                UnitTargeting.SetTarget(unit);
        }

        private void OnResourceDeposited(int oldValue, int newValue) {
            if (newValue > oldValue
                || newValue > 0) 
                return;
            
            _depositCommand.CompleteCommand(CommandQueue);
        }

        private void OnTargetDeath(UnitDeathEvent evnt) 
            => _depositCommand.CompleteCommand(CommandQueue, true);

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            _depositRange.OnUnitDetected -= OnDepositableDetected;
            _storage.OnAttributeChange -= OnResourceDeposited;
            
            _depositCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
            targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);
            _depositCommand.OnCommandComplete.RemoveListener(OnCommandComplete);
            _depositCommand = null;
        }
    }
}