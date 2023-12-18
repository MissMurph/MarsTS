using MarsTS.Commands;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class Infantry : Unit {

		public new Faction Owner { get { return squad.Owner; } }

		public InfantrySquad squad;

		[SerializeField]
		private float moveSpeed;

		private GroundDetection ground;

		protected override void Awake () {
			base.Awake();

			ground = GetComponent<GroundDetection>();
		}

		protected virtual void FixedUpdate () {
			if (ground.Grounded) {
				if (!currentPath.IsEmpty) {
					Vector3 targetWaypoint = currentPath[pathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;
					body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z));

					Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					Vector3 newVelocity = moveDirection * moveSpeed;

					body.velocity = newVelocity;
				}
				else {
					body.velocity = Vector3.zero;
				}
			}
		}

		public override Commandlet Auto (ISelectable target) {
			throw new System.NotImplementedException();
		}

		public override Command Evaluate (ISelectable target) {
			throw new System.NotImplementedException();
		}
	}
}