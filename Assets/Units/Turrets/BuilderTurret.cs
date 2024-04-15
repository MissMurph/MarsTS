using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Units {

    public class BuilderTurret : MonoBehaviour {

		[SerializeField]
		private int repairRate;

		private int repairAmount;

		private float cooldown;
		private float currentCooldown;

		[SerializeField]
		private GameObject barrel;

		public float Range { get { return sensor.Range; } }

		private IAttackable target;

		private ISelectable parent;
		private EventAgent bus;

		private AttackableSensor sensor;

		private void Awake () {
			parent = GetComponentInParent<ISelectable>();
			bus = GetComponentInParent<EventAgent>();
			sensor = GetComponent<AttackableSensor>();

			bus.AddListener<SensorUpdateEvent<IAttackable>>(OnSensorUpdate);

			cooldown = 1f / repairRate;
			repairAmount = (int)(repairRate * cooldown);
		}

		private void Update () {
			if (!NetworkManager.Singleton.IsServer) return;

			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null && commandableUnit.CurrentCommand.Name == "repair") {
				Commandlet<IAttackable> repairCommand = commandableUnit.CurrentCommand as Commandlet<IAttackable>;

				if (sensor.IsDetected(repairCommand.Target)) {
					target = repairCommand.Target;
				}
			}

			if (target == null) {
				float distance = Range;
				IAttackable currentClosest = null;

				foreach (IAttackable unit in sensor.Detected) {
					if (unit.GetRelationship(parent.Owner) == Relationship.Owned || unit.GetRelationship(parent.Owner) == Relationship.Friendly) {
						if (unit.Health >= unit.MaxHealth) break;

						float newDistance = Vector3.Distance(unit.GameObject.transform.position, transform.position);

						if (newDistance < distance) {
							currentClosest = unit;
						}
					}
				}

				if (currentClosest != null) target = currentClosest;
			}

			if (target != null && sensor.IsDetected(target) && currentCooldown <= 0) {
				Repair();
			}
		}

		private void FixedUpdate () {
			if (target != null && sensor.Detected.Contains(target)) {
				barrel.transform.LookAt(target.GameObject.transform, Vector3.up);
			}
		}

		private void Repair () {
			target.Attack(-repairAmount);
			currentCooldown += cooldown;
		}

		private void OnSensorUpdate (SensorUpdateEvent<IAttackable> _event) {
			if (_event.Detected == true) {
				if (target == null) {
					target = _event.Target;
				}
			}
			else if (ReferenceEquals(_event.Target, target)) {
				target = null;
			}
		}

		public bool IsInRange (IAttackable target) {
			return sensor.IsDetected(target);
		}
	}
}