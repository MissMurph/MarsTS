using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class ArtilleryTurret : ProjectileTurret {

		private Quaternion startingPos;

		private bool isDeployed;

		private GameObject rangeIndicator;

		protected override void Awake () {
			base.Awake();

			rangeIndicator = transform.root.Find("RangeIndicator").gameObject;
		}

		private void Start () {
			startingPos = barrel.transform.localRotation;

			bus.AddListener<DeployEvent>(OnDeploy);
			bus.AddListener<UnitSelectEvent>(OnSelect);
			bus.AddListener<UnitHoverEvent>(OnHover);
		}

		protected override void Update () {
			if (isDeployed) {
				if (currentCooldown >= 0f) {
					currentCooldown -= Time.deltaTime;
					bus.Local(new WorkEvent(bus, parent, cooldown, cooldown - currentCooldown));
				}

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
							float newDistance = Vector3.Distance(sensor.GetDetectedCollider(unit.GameObject.name).transform.position, transform.position);

							if (newDistance < distance) {
								currentClosest = unit;
							}
						}
					}

					if (currentClosest != null) target = currentClosest;
				}

				if (target != null && sensor.IsDetected(target) && currentCooldown <= 0) {
					FireProjectile(sensor.GetDetectedCollider(target.GameObject.name).transform.position);
				}
			}
			else {
				barrel.transform.localRotation = startingPos;

				if (currentCooldown >= 0f) {
					currentCooldown -= Time.deltaTime;
				}
			}
		}

		private void OnDeploy (DeployEvent _event) {
			isDeployed = _event.IsDeployed;
			rangeIndicator.SetActive(_event.IsDeployed);
		}

		private void OnSelect (UnitSelectEvent _event) {
			if (isDeployed && _event.Status) {
				rangeIndicator.SetActive(true);
			}
			else {
				rangeIndicator.SetActive(false);
			}
		}

		private void OnHover (UnitHoverEvent _event) {
			if (isDeployed && _event.Status) {
				rangeIndicator.SetActive(true);
			}
			else {
				rangeIndicator.SetActive(false);
			}
		}
	}
}