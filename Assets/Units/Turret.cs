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
				return falloff.radius;
			}
		}

		[SerializeField]
		private SphereCollider falloff;

		[SerializeField]
		private GameObject barrel;

		[SerializeField]
		private float turnRate;

		[SerializeField]
		private int damage;

		//Seconds between firing
		[SerializeField]
		private float cooldown;
		private float currentCooldown;

		private Dictionary<int, Unit> inRangeUnits;

		public Unit target;

		private Unit parent;

		private void Awake () {
			inRangeUnits = new Dictionary<int, Unit>();
			parent = GetComponentInParent<Unit>();
		}

		private void Start () {
			range.gameObject.SetActive(true);
			falloff.gameObject.SetActive(true);
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
				Debug.Log("attacking!");
				Vector3 direction = (target.transform.position - transform.position).normalized;
				Physics.Raycast(barrel.transform.position, direction, range.radius);
				Debug.DrawLine(barrel.transform.position, barrel.transform.position + (direction * range.radius), Color.cyan, 100f);
				currentCooldown = cooldown;
			}
		}

		private void FixedUpdate () {
			if (target != null && inRangeUnits.ContainsKey(target.InstanceID)) {
				
				barrel.transform.LookAt(target.transform, Vector3.up);
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (UnitCache.TryGet(other.transform.root.name, out Unit target)) {
				inRangeUnits.TryAdd(target.InstanceID, target);
			}
		}

		private void OnTriggerExit (Collider other) {
			if (UnitCache.TryGet(other.transform.root.name, out Unit target)) {
				inRangeUnits.Remove(target.InstanceID);
			}
		}
	}
}