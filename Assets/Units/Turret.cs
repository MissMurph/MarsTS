using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units.Cache;
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
		private SphereCollider range;

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
		private int damage;

		//Seconds between firing
		[SerializeField]
		protected float cooldown;
		protected float currentCooldown;

		private Dictionary<int, Unit> inRangeUnits;

		public Unit target;

		protected Unit parent;

		private void Awake () {
			inRangeUnits = new Dictionary<int, Unit>();
			parent = GetComponentInParent<Unit>();
		}

		private void Start () {
			range.gameObject.SetActive(true);
		}

		private void Update () {
			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}

			if (target == null) {
				foreach (Unit unit in inRangeUnits.Values) {
					if (unit.Relationship(parent.Owner) == Relationship.Hostile) {
						target = unit;
						break;
					}
				}
			}

			if (target != null && inRangeUnits.ContainsKey(target.InstanceID) && currentCooldown <= 0) {
				Fire();
			}
		}

		protected virtual void Fire () {
			Vector3 direction = (target.transform.position - transform.position).normalized;

			Physics.Raycast(barrel.transform.position, direction, range.radius);
			Debug.DrawLine(barrel.transform.position, barrel.transform.position + (direction * range.radius), Color.cyan, 0.1f);

			currentCooldown = cooldown;
		}

		private void FixedUpdate () {
			if (target != null && inRangeUnits.ContainsKey(target.InstanceID)) {
				
				barrel.transform.LookAt(target.transform, Vector3.up);
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (UnitCache.TryGet(other.transform.root.name, out Unit unit)) {
				inRangeUnits.TryAdd(unit.InstanceID, unit);
			}
		}

		private void OnTriggerExit (Collider other) {
			if (UnitCache.TryGet(other.transform.root.name, out Unit unit)) {
				inRangeUnits.Remove(unit.InstanceID);
				if (target != null && target.InstanceID == unit.InstanceID) target = null;
			}
		}
	}
}