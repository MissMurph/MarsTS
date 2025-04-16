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
	public class RepairReceiver : MonoBehaviour,
								  IEntityUpdate
	{
		[SerializeField] private AttackableSensor _targetTrackRange;

		private UnitPathfinder _unitPathing;
		private UnitTargetManager _unitTargeting;
		private CommandQueue _commandQueue;
		private EventAgent _eventAgent;
		private UnitOwnership _ownership;
		
		private AttackableCommandlet _repairCommand;

		private void Awake() {
			_eventAgent = GetComponent<EventAgent>();
			_commandQueue = GetComponent<CommandQueue>();
			_unitPathing = GetComponent<UnitPathfinder>();
			_unitTargeting = GetComponent<UnitTargetManager>();
			_ownership = GetComponent<UnitOwnership>();
		}

		private void Start() {
			_eventAgent.AddListener<CommandStartEvent>(ReceiveCommand);
		}

		private void ReceiveCommand(CommandStartEvent evnt) {
			if (evnt.Command is not AttackableCommandlet deserialized
				|| evnt.Command.Name != "repair") 
				return;
			
			IAttackable unit = deserialized.Target;
			if (unit.GetRelationship(_ownership.Owner) != Relationship.Owned
				&& unit.GetRelationship(_ownership.Owner) != Relationship.Friendly) return;
			
			_repairCommand = deserialized;
			_unitTargeting.SetTarget(_repairCommand.Target);
			
			_repairCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);
			targetBus.AddListener<UnitHurtEvent>(OnTargetHealed);
			targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);
			_repairCommand.Callback.AddListener(OnCommandComplete);
		}

		public void UpdateServer() {
			if (_repairCommand is null) 
				return;
			
			if (_targetTrackRange.IsDetected(_repairCommand.Target)) {
				_unitTargeting.ClearTarget();
				_unitPathing.ClearPath();
			}
			else
				_unitTargeting.SetTarget(_repairCommand.Target);
		}

		public void UpdateClient() { }

		private void OnTargetHealed(UnitHurtEvent evnt) {
			if (evnt.Targetable.Health < evnt.Targetable.MaxHealth) return;
			
			_repairCommand.CompleteCommand(_commandQueue);
		}

		private void OnTargetDeath(UnitDeathEvent evnt) => _repairCommand.CompleteCommand(_commandQueue, true);

		private void OnCommandComplete(CommandCompleteEvent evnt) {
			_repairCommand.Target.Entity.TryGetEntityComponent(out EventAgent targetBus);

			targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);
			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			_repairCommand = null;

			_unitTargeting.ClearTarget();
			_unitPathing.ClearPath();
		}

		/*public override CommandFactory Evaluate (ISelectable target) {
			if (target is IAttackable attackable
				&& (target.GetRelationship(Owner) == Relationship.Owned || target.GetRelationship(Owner) == Relationship.Friendly)
				//&& (target.GameObject.CompareTag("vehicle") || target.GameObject.CompareTag("building"))
				&& attackable.Health < attackable.MaxHealth) {
				return CommandPrimer.Get("repair");
			}

			return CommandPrimer.Get("move");
		}

		public override void AutoCommand (ISelectable target) {
			if (target is IAttackable attackable
				&& (target.GetRelationship(Owner) == Relationship.Owned || target.GetRelationship(Owner) == Relationship.Friendly)
				//&& (target.GameObject.CompareTag("vehicle") || target.GameObject.CompareTag("building"))
				&& attackable.Health < attackable.MaxHealth) {
				//CommandRegistry.Get<Repair>("repair").Construct(attackable, Player.SerializedSelected);
			}

			CommandPrimer.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		public override bool CanCommand (string key) {
			string[] splitKey = key.Split("/");
			if (splitKey[0] == "construct") return true;

			return base.CanCommand(key);
		}*/
	}
}