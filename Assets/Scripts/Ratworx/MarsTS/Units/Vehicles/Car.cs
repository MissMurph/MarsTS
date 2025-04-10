using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Turrets;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Vehicles {

	public class Car : AbstractUnit {

		[Header("Turret")]
		protected Dictionary<string, ProjectileTurret> registeredTurrets = new Dictionary<string, ProjectileTurret>();

		protected override void Awake () {
			base.Awake();


			foreach (ProjectileTurret turret in GetComponentsInChildren<ProjectileTurret>()) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			if (!NetworkManager.Singleton.IsServer) return;

			base.Update();

			if (attackTarget == null) return;

			// move this to the attack receiver
			if (registeredTurrets["turret_main"].IsInRange(AttackTarget)) {
				TrackedTarget = null;
				CurrentPath = Path.Empty;
			}
			else if (!ReferenceEquals(TrackedTarget, attackTarget.GameObject.transform)) {
				SetTarget(attackTarget.GameObject.transform);
			}
		}

		/*public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "attack":
					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.EnqueueCommand(order);
			else commands.ExecuteCommand(order);
		}

		protected override void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "attack":
					Attack(_event.Command);
					break;
				default:
					base.ExecuteOrder(_event);
					break;
			}
		}*/

		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget = deserialized.Target;

				EntityCache.TryGetEntityComponent(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		private void AttackCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized && _event.IsCancelled) {
				EntityCache.TryGetEntityComponent(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				AttackTarget = null;
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			/*EntityCache.TryGetEntityComponent(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);*/
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