using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace MarsTS.Units {

    public class ProjectileTurret : MonoBehaviour {

		[SerializeField]
		protected GameObject projectile;

		[SerializeField]
		protected int damage;

		[SerializeField]
		protected float cooldown;
		protected float currentCooldown;

		[SerializeField]
		protected GameObject barrel;

		public float Range { get { return sensor.Range; } }

		protected IAttackable target;

		protected ISelectable parent;
		protected EventAgent bus;

		protected AttackableSensor sensor;

		protected virtual void Awake () {
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
						float newDistance = Vector3.Distance(sensor.GetDetectedCollider(unit.GameObject.name).transform.position, transform.position);

						if (newDistance < distance) {
							currentClosest = unit;
						}
					}
				}

				if (currentClosest != null) target = currentClosest;
			}

			if (target != null && sensor.IsDetected(target) && currentCooldown <= 0) {
				Fire(sensor.GetDetectedCollider(target.GameObject.name).transform.position);
			}
		}

		private void FixedUpdate () {
			if (target != null && sensor.IsDetected(target)) {
				barrel.transform.LookAt(sensor.GetDetectedCollider(target.GameObject.name).transform.position, Vector3.up);
			}
		}

		protected virtual void Fire (Vector3 position) {
			Vector3 direction = (position - transform.position).normalized;

			Projectile bullet = Instantiate(projectile, barrel.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<Projectile>();

			bullet.transform.LookAt(position);

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