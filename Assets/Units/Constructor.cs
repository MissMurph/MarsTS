using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Constructor : Unit {

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

		protected Dictionary<string, Turret> registeredTurrets = new Dictionary<string, Turret>();

		[SerializeField]
		protected Turret[] turretsToRegister;

		protected ISelectable RepairTarget {
			get {
				return repairTarget;
			}
			set {
				//repairTarget = value;
				//registeredTurrets["turret_main"].target = repairTarget;

				if (repairTarget != null) {
					EntityCache.TryGet(repairTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => repairTarget = null);
				}

				repairTarget = value;
				registeredTurrets["turret_main"].target = repairTarget;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => repairTarget = null);
				}
			}
		}

		protected ISelectable repairTarget;

		protected override void Awake () {
			base.Awake();
			foreach (Turret turret in turretsToRegister) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();

			if (repairTarget == null) return;

			if (Vector3.Distance(repairTarget.GameObject.transform.position, transform.position) <= registeredTurrets["turret_main"].Range) {
				target = null;
				currentPath = Path.Empty;
			}
			else if (!ReferenceEquals(target, repairTarget.GameObject.transform)) {
				SetTarget(repairTarget.GameObject.transform);
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
			RepairTarget = null;
			switch (order.Name) {
				case "construct":
				
				break;
				case "repair":
				Repair(order);
				break;
				//This is brilliant
				default:
				base.ProcessOrder(order);
				break;
			}
		}

		protected void Repair (Commandlet order) {
			if (order.TargetType.Equals(typeof(ISelectable))) {
				Commandlet<ISelectable> deserialized = order as Commandlet<ISelectable>;
				ISelectable unit = deserialized.Target;

				if ((unit.GetRelationship(owner) == Teams.Relationship.Owned || unit.GetRelationship(owner) == Teams.Relationship.Friendly)) {
					

					RepairTarget = unit;
				}
			}
		}
	}
}