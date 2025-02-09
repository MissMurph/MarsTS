using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Tank : AbstractUnit {

		[Header("Movement")]

		[SerializeField]
		protected float topSpeed;

		[SerializeField]
		protected float currentTopSpeed;

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

		private GroundDetection ground;

		protected Dictionary<string, ProjectileTurret> registeredTurrets = new Dictionary<string, ProjectileTurret>();

		protected UnitReference<IAttackable> AttackTarget = new UnitReference<IAttackable>();

		protected override void Awake () {
			base.Awake();

			ground = GetComponent<GroundDetection>();

			currentTopSpeed = topSpeed;

			foreach (ProjectileTurret turret in GetComponentsInChildren<ProjectileTurret>()) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();

			if (AttackTarget.Get == null) return;

			if (registeredTurrets["turret_main"].IsInRange(AttackTarget.Get)) {
				TrackedTarget = null;
				CurrentPath = Path.Empty;
			}
			else if (!ReferenceEquals(TrackedTarget, AttackTarget.GameObject.transform)) {
				SetTarget(AttackTarget.GameObject.transform);
			}
		}

		protected virtual void FixedUpdate () {
			velocity = Body.velocity.sqrMagnitude;

			if (ground.Grounded) {
				if (!CurrentPath.IsEmpty) {
					Vector3 targetWaypoint = CurrentPath[PathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

					float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);
					Body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

					Vector3 currentVelocity = Body.velocity;
					Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					adjustedVelocity *= currentVelocity.magnitude;

					if (Vector3.Angle(targetDirection, transform.forward) <= angleTolerance) {
						float accelCap = 1f - (velocity / (currentTopSpeed * currentTopSpeed));

						//This moves the velocity according to the rotation of the unit
						Body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (turnSpeed * accelCap) * Time.fixedDeltaTime);

						//Relative so it can take into account the forward vector of the car
						Body.AddRelativeForce(Vector3.forward * (acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);
					}

					if (velocity > currentTopSpeed * currentTopSpeed) {
						Vector3 direction = Body.velocity.normalized;
						direction *= currentTopSpeed;
						Body.velocity = direction;
					}
				}
				else if (velocity >= 0.5f) {
					Body.AddRelativeForce(-Body.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
				}
			}
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "attack":
					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
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
		}

		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget.Set(deserialized.Target, deserialized.Target.GameObject);

				EntityCache.TryGet(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		protected override void Stop () {
			base.Stop();


		}

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			//bus.RemoveListener<CommandCompleteEvent>(AttackCancelled);

			if (_event.Command is Commandlet<IAttackable> deserialized && _event.IsCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				AttackTarget.Set(null, null);
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);

			

			//CurrentCommand = null;
		}

		public override CommandFactory Evaluate (ISelectable target) {
			if (target is IAttackable && target.GetRelationship(Owner) == Relationship.Hostile) {
				return CommandPrimer.Get("attack");
			}

			return CommandPrimer.Get("move");
		}

		public override void AutoCommand (ISelectable target) {
			if (target is IAttackable deserialized && target.GetRelationship(Owner) == Relationship.Hostile) {
				//return CommandRegistry.Get<Attack>("attack").Construct(deserialized);
			}

			//return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);

			throw new NotImplementedException();
		}
	}
}