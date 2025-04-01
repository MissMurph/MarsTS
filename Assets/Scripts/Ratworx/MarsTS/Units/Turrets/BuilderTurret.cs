using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.SafeReference;
using Ratworx.MarsTS.Units.Sensors;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets {

    public class BuilderTurret : MonoBehaviour {

		[SerializeField]
		private int repairRate;

		private int _repairAmount;

		private float _cooldown;
		private float _currentCooldown;

		[SerializeField]
		private GameObject barrel;

		public float Range => _sensor.Range;

		private UnitReference<IAttackable> _target = new UnitReference<IAttackable>();

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
					_target.Set(repairCommand.Target, repairCommand.Target.GameObject);
				}
			}

			if (_target.Get == null) {
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

				if (currentClosest != null) _target.Set(currentClosest, currentClosest.GameObject);
			}

			if (_target.Get != null && _sensor.IsDetected(_target.Get) && _currentCooldown <= 0) {
				Repair();
			}
		}

		private void FixedUpdate () {
			if (_target.Get != null && _sensor.Detected.Contains(_target.Get)) {
				barrel.transform.LookAt(_target.GameObject.transform, Vector3.up);
			}
		}

		private void Repair () {
			_target.Get.Attack(-_repairAmount);
			_currentCooldown += _cooldown;
		}

		private void OnSensorUpdate (SensorUpdateEvent<IAttackable> @event) {
			if (@event.Detected) {
				if (_target.Get == null)
				{
					_target.Set(@event.Target, @event.Target.GameObject);
				}
			}
			else if (ReferenceEquals(@event.Target, _target.Get)) {
				_target.Set(null, null);
			}
		}

		public bool IsInRange (IAttackable target) {
			return _sensor.IsDetected(target);
		}
	}
}