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

		[SerializeField]
		private float topSpeed;
		[SerializeField]
		private float reverseSpeed;
		[SerializeField]
		private float acceleration;
		[SerializeField]
		private float currentSpeed;

		[SerializeField]
		private float velocity;
		
		[SerializeField]
		private float turnSpeed;

		private float CurrentAngle {
			get {
				float angle = transform.rotation.eulerAngles.y;
				return angle;
			}
		}

		[Header("Braking")]

		[SerializeField]
		private float brakeWindowTime;

		[SerializeField]
		private float brakeForce;

		[Header("Turret")]

		protected Dictionary<string, ProjectileTurret> registeredTurrets = new Dictionary<string, ProjectileTurret>();

		protected IAttackable AttackTarget {
			get {
				return attackTarget;
			}
			set {
				if (attackTarget != null) {
					EntityCache.TryGet(attackTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<UnitDeathEvent>((_event) => AttackTarget = null);
					oldAgent.RemoveListener<EntityVisibleEvent>((_event) => {
						if (!_event.Visible) {
							SetTarget(AttackTarget.GameObject.transform.position);
							AttackTarget = null;
						}
					});
				}

				attackTarget = value;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<UnitDeathEvent>((_event) => AttackTarget = null);
					agent.AddListener<EntityVisibleEvent>((_event) => {
						if (!_event.Visible) {
							SetTarget(AttackTarget.GameObject.transform.position);
							AttackTarget = null;
						}
					});
				}
			}
		}

		protected IAttackable attackTarget;

		private GroundDetection ground;

		protected override void Awake () {
			base.Awake();

			ground = GetComponent<GroundDetection>();

			foreach (ProjectileTurret turret in GetComponentsInChildren<ProjectileTurret>()) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			if (!NetworkManager.Singleton.IsServer) return;

			base.Update();

			if (attackTarget == null) return;

			if (registeredTurrets["turret_main"].IsInRange(AttackTarget)) {
				TrackedTarget = null;
				CurrentPath = Path.Empty;
			}
			else if (!ReferenceEquals(TrackedTarget, attackTarget.GameObject.transform)) {
				SetTarget(attackTarget.GameObject.transform);
			}
		}

		protected virtual void FixedUpdate () {
			if (!NetworkManager.Singleton.IsServer) return;

			velocity = Body.velocity.sqrMagnitude;

			if (ground.Grounded) {
				if (!CurrentPath.IsEmpty) {
					Vector3 targetWaypoint = CurrentPath[PathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;

					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

					float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);

					Vector3 currentVelocity = Body.velocity;

					//float brakeThreshold = currentVelocity.magnitude * brakeWindowTime;

					Body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

					Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					adjustedVelocity *= currentVelocity.magnitude;

					float accelCap = 1f - (velocity / (topSpeed * topSpeed));

					Body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (turnSpeed * accelCap) * Time.fixedDeltaTime);

					//Relative so it can take into account the forward vector of the car
					Body.AddRelativeForce(Vector3.forward * (acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);

					if (velocity > topSpeed * topSpeed) {
						Vector3 direction = Body.velocity.normalized;
						direction *= topSpeed;
						Body.velocity = direction;
					}
				}
				else if (Body.velocity.magnitude >= 0.5f) {
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
				AttackTarget = deserialized.Target;

				EntityCache.TryGet(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		private void AttackCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized && _event.IsCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				AttackTarget = null;
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);
		}

		public override CommandFactory Evaluate (ISelectable target) {
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
		}
	}
}