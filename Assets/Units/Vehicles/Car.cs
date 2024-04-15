using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Unity.Netcode;

namespace MarsTS.Units {

	public class Car : Unit {

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
				currentPath = Path.Empty;
			}
			else if (!ReferenceEquals(TrackedTarget, attackTarget.GameObject.transform)) {
				SetTarget(attackTarget.GameObject.transform);
			}
		}

		protected virtual void FixedUpdate () {
			if (!NetworkManager.Singleton.IsServer) return;

			velocity = body.velocity.sqrMagnitude;

			if (ground.Grounded) {
				if (!currentPath.IsEmpty) {
					Vector3 targetWaypoint = currentPath[pathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;

					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

					float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);

					Vector3 currentVelocity = body.velocity;

					//float brakeThreshold = currentVelocity.magnitude * brakeWindowTime;

					body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

					Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					adjustedVelocity *= currentVelocity.magnitude;

					float accelCap = 1f - (velocity / (topSpeed * topSpeed));

					body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (turnSpeed * accelCap) * Time.fixedDeltaTime);

					//Relative so it can take into account the forward vector of the car
					body.AddRelativeForce(Vector3.forward * (acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);

					if (velocity > topSpeed * topSpeed) {
						Vector3 direction = body.velocity.normalized;
						direction *= topSpeed;
						body.velocity = direction;
					}
				}
				else if (body.velocity.magnitude >= 0.5f) {
					body.AddRelativeForce(-body.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
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
			if (_event.Command is Commandlet<IAttackable> deserialized && _event.CommandCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				AttackTarget = null;
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);
		}

		public override CommandFactory Evaluate (ISelectable target) {
			if (target is IAttackable && target.GetRelationship(Owner) == Relationship.Hostile) {
				return CommandRegistry.Get("attack");
			}

			return CommandRegistry.Get("move");
		}

		public override void AutoCommand (ISelectable target) {
			if (target is IAttackable deserialized && target.GetRelationship(Owner) == Relationship.Hostile) {
				CommandRegistry.Get<Attack>("attack").Construct(deserialized, Player.SerializedSelected);
			}

			CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position, Player.SerializedSelected);
		}
	}
}