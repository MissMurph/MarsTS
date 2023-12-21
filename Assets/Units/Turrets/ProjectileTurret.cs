using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using UnityEngine;

namespace MarsTS.Units {

    public class ProjectileTurret : MonoBehaviour {

		[SerializeField]
		private GameObject projectile;

		[SerializeField]
		private int damage;

		[SerializeField]
		protected float cooldown;
		protected float currentCooldown;

		[SerializeField]
		private GameObject barrel;

		public float Range { get { return sensor.Range; } }

		protected IAttackable target;

		protected ISelectable parent;
		protected EventAgent bus;

		protected AttackableSensor sensor;

		private void Awake () {
			parent = GetComponentInParent<ISelectable>();
			bus = GetComponentInParent<EventAgent>();
			sensor = GetComponent<AttackableSensor>();

			bus.AddListener<SensorUpdateEvent<IAttackable>>(OnSensorUpdate);
		}

		protected virtual void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
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
						float newDistance = Vector3.Distance(unit.GameObject.transform.position, transform.position);

						if (newDistance < distance) {
							currentClosest = unit;
						}
					}
				}

				if (currentClosest != null) target = currentClosest;
			}

			if (target != null && sensor.IsDetected(target) && currentCooldown <= 0) {
				Fire();
			}
		}

		private void FixedUpdate () {
			if (target != null && sensor.IsDetected(target)) {
				barrel.transform.LookAt(target.GameObject.transform, Vector3.up);
			}
		}

		protected void Fire () {
			Vector3 direction = (target.GameObject.transform.position - transform.position).normalized;

			Projectile bullet = Instantiate(projectile, barrel.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<Projectile>();

			bullet.transform.LookAt(target.GameObject.transform.position);

			bullet.Init(parent, (success, unit) => { if (success) unit.Attack(damage); });

			currentCooldown += cooldown;
		}

		private void OnSensorUpdate (SensorUpdateEvent<IAttackable> _event) {
			if (_event.Detected == true) {
				if (target == null && _event.Target.GetRelationship(parent.Owner) == Relationship.Hostile) {
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