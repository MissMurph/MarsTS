using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class DepositSensor : MonoBehaviour {

		public float Range { get { return range.radius; } }

		[SerializeField]
		protected SphereCollider range;

		private Dictionary<string, IDepositable> inRangeBanks = new Dictionary<string, IDepositable>();

		private void Awake () {
			range = GetComponent<SphereCollider>();

			foreach (Collider collider in transform.parent.Find("Model").GetComponentsInChildren<Collider>()) {
				Physics.IgnoreCollision(range, collider);
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp) && entityComp.TryGet(out IDepositable bank)) {
				entityComp.Get<EventAgent>("eventAgent").AddListener<EntityDeathEvent>((_event) => OutOfRange(bank));
				inRangeBanks.TryAdd(other.transform.root.name, bank);
			}
		}

		private void OnTriggerExit (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out IDepositable bank)) {
				OutOfRange(bank);
			}
		}

		public bool IsInRange (IDepositable unit) {
			return inRangeBanks.ContainsKey(unit.GameObject.transform.root.name);
		}

		protected virtual void OutOfRange (IDepositable unit) {
			string name = unit.GameObject.transform.root.name;
			inRangeBanks.Remove(name);
		}
	}
}