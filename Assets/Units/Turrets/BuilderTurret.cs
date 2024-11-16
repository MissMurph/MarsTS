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

		private int _repairAmount;

		private float _cooldown;
		private float _currentCooldown;

		[SerializeField]
		private GameObject barrel;

		public float Range => _sensor.Range;

		private IAttackable _target;

		private ISelectable _parent;
		private EventAgent _bus;

		private AttackableSensor _sensor;

		private void Awake () {
			_parent = GetComponentInParent<ISelectable>();
			_bus = GetComponentInParent<EventAgent>();
			_sensor = GetComponent<AttackableSensor>();

			_bus.AddListener<SensorUpdateEvent<IAttackable>>(OnSensorUpdate);

			_cooldown = 1f / repairRate;
			_repairAmount = (int)(repairRate * _cooldown);
		}

		private void Update () {
			if (!NetworkManager.Singleton.IsServer) return;

			if (_currentCooldown >= 0f) {
				_currentCooldown -= Time.deltaTime;
			}

			if (_parent is ICommandable commandableUnit && commandableUnit.CurrentCommand != null && commandableUnit.CurrentCommand.Name == "repair") {
				var repairCommand = commandableUnit.CurrentCommand as Commandlet<IAttackable>;

				if (_sensor.IsDetected(repairCommand.Target)) {
					_target = repairCommand.Target;
				}
			}

			if (_target == null) {
				float distance = Range;
				IAttackable currentClosest = null;

				foreach (IAttackable unit in _sensor.Detected) {
					if (unit.GetRelationship(_parent.Owner) == Relationship.Owned || unit.GetRelationship(_parent.Owner) == Relationship.Friendly) {
						if (unit.Health >= unit.MaxHealth) break;

						float newDistance = Vector3.Distance(unit.GameObject.transform.position, transform.position);

						if (newDistance < distance) {
							currentClosest = unit;
						}
					}
				}

				if (currentClosest != null) _target = currentClosest;
			}

			if (_target != null && _sensor.IsDetected(_target) && _currentCooldown <= 0) {
				Repair();
			}
		}

		private void FixedUpdate () {
			if (_target != null && _sensor.Detected.Contains(_target)) {
				barrel.transform.LookAt(_target.GameObject.transform, Vector3.up);
			}
		}

		private void Repair () {
			_target.Attack(-_repairAmount);
			_currentCooldown += _cooldown;
		}

		private void OnSensorUpdate (SensorUpdateEvent<IAttackable> @event) {
			if (@event.Detected == true) {
				if (_target == null) {
					_target = @event.Target;
				}
			}
			else if (ReferenceEquals(@event.Target, _target)) {
				_target = null;
			}
		}

		public bool IsInRange (IAttackable target) {
			return _sensor.IsDetected(target);
		}
	}
}