using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class Car : Unit {

		[SerializeField]
		private float topSpeed;
		[SerializeField]
		private float reverseSpeed;
		[SerializeField]
		private float acceleration;

		private float currentSpeed;

		
		[SerializeField]
		private float turnSpeed;

		[SerializeField]
		private float targetAngle;

		[SerializeField]
		private Vector3 targetDirection;

		private float CurrentAngle {
			get {
				float angle = transform.rotation.eulerAngles.y;
				currentAngle = angle;
				return angle;
			}
		}

		public float currentAngle;

		protected override void Update () {
			base.Update();

			
		}

		protected virtual void FixedUpdate () {
			if (!currentPath.IsEmpty) {
				Vector3 targetWaypoint = currentPath[pathIndex];
				targetDirection = new Vector3 (targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
				currentSpeed += acceleration * Time.fixedDeltaTime;
				currentSpeed = Mathf.Min(currentSpeed, topSpeed);

				targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;
				

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

				float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);

				//if (CurrentAngle > 0) posOrNeg *= -1;

				body.MoveRotation(Quaternion.Euler(transform.rotation.x, newAngle, transform.rotation.z));
				body.AddRelativeForce(Vector3.forward * topSpeed * Time.fixedDeltaTime, ForceMode.Force);
				//body.AddRelativeForce(Vector3.forward * topSpeed * Time.fixedDeltaTime, ForceMode.Force);
			}
		}
	}
}