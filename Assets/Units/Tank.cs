using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Tank : Unit {

		[Header("Movement")]

		[SerializeField]
		private float topSpeed;

		[SerializeField]
		private float acceleration;

		[SerializeField]
		private float turnSpeed;

		private float CurrentAngle {
			get {
				float angle = transform.rotation.eulerAngles.y;
				return angle;
			}
		}

		[SerializeField]
		private float angleTolerance;

		private float velocity;

		[Header("Turrets")]

		protected Dictionary<string, AbstractTurret> registeredTurrets = new Dictionary<string, AbstractTurret>();

		[SerializeField]
		protected AbstractTurret[] turretsToRegister;

		protected IAttackable AttackTarget {
			get {
				return attackTarget;
			}
			set {
				if (attackTarget != null) {
					EntityCache.TryGet(attackTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => AttackTarget = null);
				}

				attackTarget = value;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => AttackTarget = null);
				}
			}
		}

		protected IAttackable attackTarget;

		protected override void Awake () {
			base.Awake();
			foreach (AbstractTurret turret in turretsToRegister) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();

			if (attackTarget == null) return;

			if (Vector3.Distance(attackTarget.GameObject.transform.position, transform.position) <= registeredTurrets["turret_main"].Range) {
				TrackedTarget = null;
				currentPath = Path.Empty;
			}
			else if (!ReferenceEquals(TrackedTarget, attackTarget.GameObject.transform)) {
				SetTarget(attackTarget.GameObject.transform);
			}
		}

		protected virtual void FixedUpdate () {
			velocity = body.velocity.magnitude;

			if (!currentPath.IsEmpty) {
				Vector3 targetWaypoint = currentPath[pathIndex];

				Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
				float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

				float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);
				body.MoveRotation(Quaternion.Euler(transform.rotation.x, newAngle, transform.rotation.z));

				Vector3 currentVelocity = body.velocity;
				Vector3 adjustedVelocity = transform.forward * currentVelocity.magnitude;

				if (Vector3.Angle(targetDirection, transform.forward) <= angleTolerance) {
					float accelCap = 1f - (velocity / topSpeed);

					body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (turnSpeed * accelCap) * Time.fixedDeltaTime);

					//Relative so it can take into account the forward vector of the car
					body.AddRelativeForce(Vector3.forward * (acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);
				}
			}
			else if (velocity >= 0.5f) {
				body.AddRelativeForce(-body.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
			}
		}

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "attack":
					CurrentCommand = order;
					Attack(order);
					break;
				default:
					base.ProcessOrder(order);
					break;
			}
		}

		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget = deserialized.Target;

				EntityCache.TryGet(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<EntityDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		protected override void Stop () {
			base.Stop();


		}

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			//bus.RemoveListener<CommandCompleteEvent>(AttackCancelled);

			if (_event.Command is Commandlet<IAttackable> deserialized && _event.CommandCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

				AttackTarget = null;
			}
		}

		private void OnTargetDeath (EntityDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);

			bus.Global(newEvent);

			CurrentCommand = null;
		}

		public override Command Evaluate (ISelectable target) {
			if (target is IAttackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get("attack");
			}

			return CommandRegistry.Get("move");
		}

		public override Commandlet Auto (ISelectable target) {
			if (target is IAttackable deserialized && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get<Attack>("attack").Construct(deserialized);
			}

			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}
	}
}