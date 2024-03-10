using MarsTS.Commands;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarsTS.Events;

namespace MarsTS.Units {

    public class SubmachineGun : ProjectileTurret {

		//Amount of bullets fired per burst
		[SerializeField]
		private int burstCount;
		private int firedCount;

		[SerializeField]
		private float burstCooldown;
		private float currentBurstCooldown;

		private bool isSneaking;

		private void Start () {
			bus.AddListener<SneakEvent>(OnSneak);
			isSneaking = false;
		}

		protected override void Update () {
			if (currentCooldown >= 0f) currentCooldown -= Time.deltaTime;
			if (currentBurstCooldown >= 0f) currentBurstCooldown -= Time.deltaTime;

			if (parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null && commandableUnit.CurrentCommand.Name == "attack") {
				Commandlet<IAttackable> attackCommand = commandableUnit.CurrentCommand as Commandlet<IAttackable>;

				if (sensor.IsDetected(attackCommand.Target)) {
					target = attackCommand.Target;
				}
			}

			if (target == null) {
				float distance = sensor.Range * sensor.Range;
				IAttackable currentClosest = null;

				foreach (IAttackable unit in sensor.Detected) {
					if (unit.GetRelationship(parent.Owner) == Relationship.Hostile) {
						float newDistance = Vector3.Distance(sensor.GetDetectedCollider(target.GameObject.name).transform.position, transform.position);

						if (newDistance < distance) {
							currentClosest = unit;
						}
					}
				}
				if (currentClosest != null) target = currentClosest;
			}

			if (!isSneaking && target != null && sensor.IsDetected(target) && currentBurstCooldown <= 0 && currentCooldown <= 0) {
				Fire(sensor.GetDetectedCollider(target.GameObject.name).transform.position);
				firedCount++;

				if (firedCount >= burstCount) {
					currentBurstCooldown += burstCooldown;
					firedCount = 0;
				}
			}
		}

		private void OnSneak (SneakEvent _event) {
			isSneaking = _event.IsSneaking;
		}
	}
}