using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Vision;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public abstract class AbstractSensor<T> : MonoBehaviour {

        public float Range { get { return range.radius; } }

		public List<T> Detected {
			get {
				List<T> output = new List<T>();

				foreach (T t in detected.Values) {
					output.Add(t);
				}

				return output;
			}
		}

		protected SphereCollider range;

		protected Dictionary<string, T> inRange = new Dictionary<string, T>();

		protected Dictionary<string, T> detected = new Dictionary<string, T>();

		protected ISelectable parent;

		protected EventAgent bus;

		protected bool initialized;

		protected void Awake () {
			range = GetComponent<SphereCollider>();
			range.enabled = false;
			bus = GetComponentInParent<EventAgent>();
			parent = GetComponentInParent<ISelectable>();

			foreach (Collider collider in transform.root.Find("Model").GetComponentsInChildren<Collider>()) {
				Physics.IgnoreCollision(range, collider);
			}

			bus.AddListener<EntityInitEvent>(OnEntityInit);
		}

		protected void Start () {
			EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
		}

		protected void OnEntityInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Pre) return;
			initialized = true;
			range.enabled = true;
		}

		protected virtual void OnTriggerEnter (Collider other) {
			if (!initialized) return;
			
			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp) 
				&& entityComp.TryGet(out T target)) {
				EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

				targetBus.AddListener<EntityDeathEvent>((_event) => OutOfRange(_event.Unit.GameObject.name));
				//targetBus.AddListener<UnitVisibleEvent>((_event) => OnVisionUpdate(_event));

				inRange[other.transform.root.name] = target;

				if (GameVision.IsVisible(other.transform.root.gameObject, parent.Owner.VisionMask)) {
					detected[other.transform.root.name] = target;
					bus.Local(new SensorUpdateEvent<T>(bus, target, true));
				}
			}
		}

		protected virtual void OnTriggerExit (Collider other) {
			if (!initialized) return;

			if (EntityCache.TryGet(other.transform.root.name, out T target)) {
				OutOfRange(other.transform.root.name);
			}
		}

		protected virtual void OnVisionUpdate (VisionUpdateEvent _event) {
			foreach (KeyValuePair<string, T> inRangeUnit in inRange) {
				if (GameVision.IsVisible(inRangeUnit.Key, parent.Owner.VisionMask)) {
					detected[inRangeUnit.Key] = inRange[inRangeUnit.Key];
					bus.Local(new SensorUpdateEvent<T>(bus, detected[inRangeUnit.Key], true));
				}
				else if (detected.ContainsKey(inRangeUnit.Key)) {
					T toRemove = detected[inRangeUnit.Key];
					detected.Remove(inRangeUnit.Key);
					bus.Local(new SensorUpdateEvent<T>(bus, toRemove, false));
				}
			}
		}

		public virtual bool IsDetected (string name) {
			return detected.ContainsKey(name);
		}

		public abstract bool IsDetected (T unit);

		protected virtual void OutOfRange (string name) {
			if (!inRange.ContainsKey(name)) return;

			EntityCache.TryGet(name, out Entity entityComp);

			EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

			targetBus.RemoveListener<EntityDeathEvent>((_event) => OutOfRange(_event.Unit.GameObject.name));
			//targetBus.RemoveListener<UnitVisibleEvent>((_event) => OnVisionUpdate(_event));

			T toRemove = inRange[name];

			if (detected.ContainsKey(name)) {
				detected.Remove(name);
				bus.Local(new SensorUpdateEvent<T>(bus, toRemove, false));
			}
			
			inRange.Remove(name);
		}
	}
}