using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public abstract class AbstractTurret : MonoBehaviour {

		public float Range { get { return range.radius; } }

		[SerializeField]
		protected SphereCollider range;

		public float Falloff { get { return falloff; } }

		[SerializeField]
		private float falloff;

		[SerializeField]
		protected GameObject barrel;

		protected Dictionary<string, ISelectable> inRangeUnits;

		protected ISelectable target;

		protected ISelectable parent;
		protected EventAgent bus;

		protected virtual void Awake () {
			range = transform.Find("Range").GetComponent<SphereCollider>();
			inRangeUnits = new Dictionary<string, ISelectable>();
			parent = GetComponentInParent<ISelectable>();
			bus = GetComponentInParent<EventAgent>();
			bus.AddListener<EntityInitEvent>(OnEntityInit);

			foreach (Collider collider in parent.GameObject.transform.Find("Model").GetComponentsInChildren<Collider>()) {
				Physics.IgnoreCollision(range, collider);
			}
		}

		private void OnEntityInit (EntityInitEvent _event) {
			range.gameObject.SetActive(true);
		}

		private void FixedUpdate () {
			if (target != null && inRangeUnits.ContainsKey(target.GameObject.transform.root.name)) {
				barrel.transform.LookAt(target.GameObject.transform, Vector3.up);
			}
		}

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp) && entityComp.TryGet(out ISelectable unit)) {
				entityComp.Get<EventAgent>("eventAgent").AddListener<EntityDeathEvent>((_event) => OutOfRange(unit));
				inRangeUnits.TryAdd(other.transform.root.name, unit);
			}
		}

		private void OnTriggerExit (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out ISelectable unit)) {
				OutOfRange(unit);
			}
		}

		public virtual bool IsInRange (ISelectable unit) {
			return inRangeUnits.ContainsKey(unit.GameObject.transform.root.name);
		}

		protected virtual void OutOfRange (ISelectable unit) {
			string name = unit.GameObject.transform.root.name;
			inRangeUnits.Remove(name);
			if (target != null && target.GameObject.transform.root.name == name) target = null;
		}
	}
}