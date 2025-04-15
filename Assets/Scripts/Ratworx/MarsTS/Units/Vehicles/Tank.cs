using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.SafeReference;
using Ratworx.MarsTS.Units.Turrets;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Vehicles {

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
	}
}