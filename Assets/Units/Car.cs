using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

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

		protected Dictionary<string, Turret> registeredTurrets = new Dictionary<string, Turret>();

		[SerializeField]
		protected Turret[] turretsToRegister;

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
				registeredTurrets["turret_main"].target = attackTarget;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => AttackTarget = null);
				}
			}
		}

		protected IAttackable attackTarget;

		protected override void Awake () {
			base.Awake();
			foreach (Turret turret in turretsToRegister) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();

			if (attackTarget == null) return;

			if (Vector3.Distance(transform.position, attackTarget.GameObject.transform.position) >= registeredTurrets["turret_main"].Range
				&& !ReferenceEquals(target, attackTarget.GameObject.transform)) {
				SetTarget(attackTarget.GameObject.transform);
			}
			else if (target == attackTarget.GameObject.transform) {
				Stop();
			}
		}

		protected virtual void FixedUpdate () {
			velocity = body.velocity.magnitude;

			if (!currentPath.IsEmpty) {
				Vector3 targetWaypoint = currentPath[pathIndex];

				//Vector3 difference = ;

				Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;

				//float distance = difference.magnitude;


				//currentSpeed += acceleration * Time.fixedDeltaTime;
				//currentSpeed = Mathf.Min(currentSpeed, topSpeed);

				float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;


				/*float distToMin = Mathf.Abs(targetAngle);
				float distToMax = 180f - distToMin;
				
				float currentDistToMin = Mathf.Abs(CurrentAngle);
				float currentDistToMax = 180f - currentDistToMin;

				float totalMinDiff = distToMin + currentDistToMin;
				float totalMaxDiff = distToMax + currentDistToMax;

				int sameSide = (int)(Mathf.Sign(targetAngle) * Mathf.Sign(CurrentAngle));

				int posOrNeg = 1;

				float finalDist = totalMinDiff;

				if (totalMaxDiff < totalMinDiff) {
					posOrNeg *= -1;
					finalDist = totalMaxDiff;
				}*/

				//if (CurrentAngle > 0) posOrNeg *= -1;

				float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);

				Vector3 currentVelocity = body.velocity;

				float brakeThreshold = currentVelocity.magnitude * brakeWindowTime;

				body.MoveRotation(Quaternion.Euler(transform.rotation.x, newAngle, transform.rotation.z));

				Vector3 adjustedVelocity = transform.forward * currentVelocity.magnitude;

				

				//Mathf.Lerp(currentSpeed, 0, distance * brakeWindowTime / brakeThreshold);

				float accelCap = 1f - (velocity / topSpeed);

				body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (turnSpeed * accelCap) * Time.fixedDeltaTime);

				//Relative so it can take into account the forward vector of the car
				body.AddRelativeForce(Vector3.forward * (acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);


				//body.AddRelativeForce(Vector3.forward * topSpeed * Time.fixedDeltaTime, ForceMode.Force);

				
			}
			else if (body.velocity.magnitude >= 0.5f) {
				body.AddRelativeForce(-body.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
			}
		}

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "attack":
				Attack(order);
				break;
				default:
				base.ProcessOrder(order);
				break;
			}
		}

		protected void Attack (Commandlet order) {
			if (order is Commandlet<ISelectable> deserialized && deserialized.Target is IAttackable target) {
				AttackTarget = target;

				EntityCache.TryGet(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<EntityDeathEvent>(OnTargetDeath);

				order.Callback.AddListener((_event) => {
					targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);
				});
			}
		}

		private void OnTargetDeath (EntityDeathEvent _event) {
			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);

			bus.Global(newEvent);

			CurrentCommand = null;
		}
	}
}