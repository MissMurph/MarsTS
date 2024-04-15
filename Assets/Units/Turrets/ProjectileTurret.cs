using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Units {

    public class ProjectileTurret : NetworkBehaviour {

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
			if (!NetworkManager.Singleton.IsServer) return;

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
				FireProjectile(sensor.GetDetectedCollider(target.GameObject.name).transform.position);
			}
		}

		private void FixedUpdate () {
			if (target != null && sensor.IsDetected(target)) {
				barrel.transform.LookAt(sensor.GetDetectedCollider(target.GameObject.name).transform.position, Vector3.up);
			}
		}

		protected virtual void FireProjectile (Vector3 _position) {
			if (NetworkManager.Singleton.IsServer) FireProjectileClientRpc(_position);

			Vector3 direction = (_position - transform.position).normalized;

			Projectile bullet = Instantiate(projectile, barrel.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<Projectile>();

			bullet.transform.LookAt(_position);

			bullet.Init(parent, OnHit);

			currentCooldown += cooldown;
		}

		[Rpc(SendTo.NotServer)]
		protected virtual void FireProjectileClientRpc (Vector3 _position) {
			FireProjectile(_position);
		}

		protected virtual void OnHit (bool _success, IAttackable _unit) {
			if (NetworkManager.Singleton.IsServer && _success) {
				_unit.Attack(damage);
			}
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