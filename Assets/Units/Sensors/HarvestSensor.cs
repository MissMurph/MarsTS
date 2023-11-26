using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class HarvestSensor : MonoBehaviour {
		public float Range { get { return range.radius; } }

		[SerializeField]
		protected SphereCollider range;

		private Dictionary<string, IHarvestable> inRange = new Dictionary<string, IHarvestable>();

		private void Awake () {
			range = GetComponent<SphereCollider>();

			foreach (Collider collider in transform.parent.Find("Model").GetComponentsInChildren<Collider>()) {
				Physics.IgnoreCollision(range, collider);
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp) && entityComp.TryGet(out IHarvestable deposit)) {
				entityComp.Get<EventAgent>("eventAgent").AddListener<EntityDeathEvent>((_event) => OutOfRange(deposit));
				inRange.TryAdd(other.transform.root.name, deposit);
			}
		}

		private void OnTriggerExit (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out IHarvestable deposit)) {
				OutOfRange(deposit);
			}
		}

		public bool IsInRange (IHarvestable unit) {
			return inRange.ContainsKey(unit.GameObject.transform.root.name);
		}

		protected virtual void OutOfRange (IHarvestable unit) {
			string name = unit.GameObject.transform.root.name;
			inRange.Remove(name);
		}
	}
}