using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class Turret : MonoBehaviour {

		public float Range {
			get {
				return range.radius;
			}
		}

		[SerializeField]
		protected SphereCollider range;

		public float Falloff {
			get {
				return falloff;
			}
		}

		[SerializeField]
		private float falloff;

		[SerializeField]
		protected GameObject barrel;

		[SerializeField]
		private float turnRate;

		[SerializeField]
		protected int damage;

		//Seconds between firing
		[SerializeField]
		protected float cooldown;
		protected float currentCooldown;

		protected Dictionary<string, IAttackable> inRangeUnits;

		public IAttackable target;

		protected Unit parent;
		protected EventAgent eventAgent;

		private void Awake () {
			inRangeUnits = new Dictionary<string, IAttackable>();
			parent = GetComponentInParent<Unit>();
			eventAgent = GetComponentInParent<EventAgent>();
			eventAgent.AddListener<EntityInitEvent>(OnEntityInit);

			foreach (Collider collider in parent.transform.Find("Model").GetComponentsInChildren<Collider>()) {
				Physics.IgnoreCollision(range, collider);
			}
		}

		private void OnEntityInit (EntityInitEvent _event) {
			range.gameObject.SetActive(true);
		}

		protected virtual void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (target == null) {
				foreach (IAttackable unit in inRangeUnits.Values) {
					if (unit.GetRelationship(parent.Owner) == Relationship.Hostile) {
						target = unit;
						break;
					}
				}
			}

			if (target != null && inRangeUnits.ContainsKey(target.GameObject.transform.root.name) && currentCooldown <= 0) {
				Fire();
			}
		}

		protected virtual void Fire () {
			Vector3 direction = (target.GameObject.transform.position - transform.position).normalized;

			Physics.Raycast(barrel.transform.position, direction, range.radius);
			Debug.DrawLine(barrel.transform.position, barrel.transform.position + (direction * range.radius), Color.cyan, 0.1f);

			target.Attack(damage);

			currentCooldown = cooldown;
		}

		private void FixedUpdate () {
			if (target != null && inRangeUnits.ContainsKey(target.GameObject.transform.root.name)) {
				
				barrel.transform.LookAt(target.GameObject.transform, Vector3.up);
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp) && entityComp.TryGet(out ISelectable unit) && unit is IAttackable target) {
				entityComp.Get<EventAgent>("eventAgent").AddListener<EntityDeathEvent>((_event) => OutOfRange(target));
				inRangeUnits.TryAdd(other.transform.root.name, target);
			}
		}

		private void OnTriggerExit (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out ISelectable unit) && unit is IAttackable target) {
				OutOfRange(target);
			}
		}

		private void OutOfRange (IAttackable unit) {
			string name = unit.GameObject.transform.root.name;
			inRangeUnits.Remove(name);
			if (target != null && target.GameObject.transform.root.name == name) target = null;
		}
	}
}