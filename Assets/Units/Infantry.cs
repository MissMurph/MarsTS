using MarsTS.Commands;
using MarsTS.Events;
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

		[SerializeField]
		private float sneakSpeed;

		private float currentSpeed;

		private GroundDetection ground;

		private bool isSneaking;

		protected override void Awake () {
			base.Awake();

			ground = GetComponent<GroundDetection>();

			currentSpeed = moveSpeed;
		}

		protected virtual void FixedUpdate () {
			if (ground.Grounded) {
				if (!currentPath.IsEmpty) {
					Vector3 targetWaypoint = currentPath[pathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;
					body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z));

					Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					Vector3 newVelocity = moveDirection * currentSpeed;

					body.velocity = newVelocity;
				}
				else {
					body.velocity = Vector3.zero;
				}
			}
		}

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "sneak":
				Sneak();
				break;
				default:
				base.ProcessOrder(order);
				break;
			}
		}

		private void Sneak () {
			if (isSneaking) {
				isSneaking = false;
				currentSpeed = moveSpeed;
			}
			else {
				isSneaking = true;
				currentSpeed = sneakSpeed;
			}

			bus.Local(new SneakEvent(bus, this, isSneaking));
		}

		public override Commandlet Auto (ISelectable target) {
			throw new System.NotImplementedException();
		}

		public override Command Evaluate (ISelectable target) {
			throw new System.NotImplementedException();
		}
	}
}