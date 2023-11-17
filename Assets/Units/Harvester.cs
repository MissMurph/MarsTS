using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class Harvester : Unit {

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

		protected Dictionary<string, HarvesterTurret> registeredTurrets = new Dictionary<string, HarvesterTurret>();

		[SerializeField]
		protected HarvesterTurret[] turretsToRegister;

		public IHarvestable HarvestTarget {
			get {
				return harvestTarget;
			}
			set {
				if (harvestTarget != null) {
					EntityCache.TryGet(harvestTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => harvestTarget = null);
				}

				harvestTarget = value;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => harvestTarget = null);
				}
			}
		}

		protected IHarvestable harvestTarget;

		protected ResourceStorage storageComp;

		protected override void Awake () {
			base.Awake();

			storageComp = GetComponent<ResourceStorage>();

			foreach (HarvesterTurret turret in turretsToRegister) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();

			if (harvestTarget == null) return;

			if (registeredTurrets["turret_main"].IsInRange(HarvestTarget as ISelectable)) {
				TrackedTarget = null;
				currentPath = Path.Empty;
			}
			else if (!ReferenceEquals(TrackedTarget, harvestTarget.GameObject.transform)) {
				SetTarget(harvestTarget.GameObject.transform);
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
			HarvestTarget = null;
			switch (order.Name) {
				case "harvest":
					CurrentCommand = order;
					Harvest(order);
					break;
				default:
					base.ProcessOrder(order);
					break;
			}
		}

		protected void Harvest (Commandlet order) {
			if (order is Commandlet<IHarvestable> deserialized) {
				HarvestTarget = deserialized.Target;
			}
		}

		public override Command Evaluate (ISelectable target) {
			if (target is IHarvestable) {
				return CommandRegistry.Get("harvest");
			}

			if (target is IDepositable) {

			}

			return CommandRegistry.Get("move");
		}

		public override Commandlet Auto (ISelectable target) {
			if (target is IHarvestable harvestable
				&& storageComp.Amount < storageComp.Capacity
				&& harvestable.StoredAmount > 0) {
				return CommandRegistry.Get<Harvest>("harvest").Construct(harvestable);
			}

			if (target is IDepositable deserialized
				&& storageComp.Amount > 0) {

			}

			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}
	}
}