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
	public class RepairReceiver : AbstractCommandReceiver<AttackableCommandlet>
	{
		[SerializeField] private AttackableSensor _targetTrackRange;
		
		private AttackableCommandlet _repairCommand;

		private void Start() {
			_targetTrackRange.OnUnitDetected += OnUnitDetected;
		}

		public override bool CanCommand => true;
		public override bool IsActive => false;
		public override float Cooldown => 0f;

		public override void ReceiveCommand(AttackableCommandlet command) {
			IAttackable unit = command.Target;
			
			if (unit.GetRelationship(Ownership.Owner) != Relationship.Owned
				&& unit.GetRelationship(Ownership.Owner) != Relationship.Friendly) return;
			
			_repairCommand = command;
			UnitTargeting.SetTarget(_repairCommand.Target);
			
			_repairCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
			targetBus.AddListener<UnitHurtEvent>(OnTargetHealed);
			targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);
			_repairCommand.OnCommandComplete.AddListener(OnCommandComplete);
		}

		public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity) {
			if (!entity.TryGetEntityComponent(out IAttackable attackable)
				|| attackable.GetRelationship(Ownership.Owner) == Relationship.Hostile
				|| attackable.GetRelationship(Ownership.Owner) == Relationship.Neutral
				|| !attackable.GameObject.CompareTag("Vehicle")
				|| !attackable.GameObject.CompareTag("Building")
				|| attackable.Health >= attackable.MaxHealth)
				return (false, null);

			return (true, CommandPrimer.Get(CommandKey));
		}

		private void OnUnitDetected(IAttackable unit, bool detected) {
			if (_repairCommand is null
				|| unit.Entity != _repairCommand.Target.Entity) 
				return;

			if (detected) {
				UnitPathing.ClearPath();
				UnitTargeting.ClearTarget();
			}
			else
				UnitTargeting.SetTarget(unit);
		}

		private void OnTargetHealed(UnitHurtEvent evnt) {
			if (evnt.Attackable.Health < evnt.Attackable.MaxHealth) return;
			
			_repairCommand.CompleteCommand(CommandQueue);
		}

		private void OnTargetDeath(UnitDeathEvent evnt) => _repairCommand.CompleteCommand(CommandQueue, true);

		private void OnCommandComplete(CommandCompleteEvent evnt) {
			_repairCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);

			targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);
			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			_repairCommand = null;

			UnitTargeting.ClearTarget();
			UnitPathing.ClearPath();
		}

		// TODO: Move to Construct Receiver
		/*public override bool CanCommand (string key) {
			string[] splitKey = key.Split("/");
			if (splitKey[0] == "construct") return true;

			return base.CanCommand(key);
		}*/
	}
}