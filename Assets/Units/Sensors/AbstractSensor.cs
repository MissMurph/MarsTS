using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Vision;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

		public List<T> InRange {
			get {
				List<T> output = new List<T>();

				foreach (T t in inRange.Values) {
					output.Add(t);
				}

				return output;
			}
		}

		public List<GameObject> InRangeColliders {
			get {
				List<GameObject> output = new List<GameObject>();

				foreach (HashSet<GameObject> table in colliders.Values) {
					output.AddRange(table);
				}

				return output;
			}
		}

		protected SphereCollider range;

		protected Dictionary<string, T> inRange = new Dictionary<string, T>();

		protected Dictionary<string, T> detected = new Dictionary<string, T>();

		protected Dictionary<string, HashSet<GameObject>> colliders = new Dictionary<string, HashSet<GameObject>>();

		protected ISelectable parent;

		protected EventAgent bus;

		protected bool initialized;

		protected virtual void Awake () {
			range = GetComponent<SphereCollider>();
			range.enabled = false;
			bus = GetComponentInParent<EventAgent>();
			parent = GetComponentInParent<ISelectable>();

			foreach (Collider collider in transform.root.GetComponentsInChildren<Collider>()) {
				Physics.IgnoreCollision(range, collider);
			}

			bus.AddListener<EntityInitEvent>(OnEntityInit);
		}

		protected virtual void Start () {
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

				targetBus.AddListener<EntityDestroyEvent>(OnEntityDestroy);

				inRange[other.transform.root.name] = target;
				//colliders[other.transform.root.name] = other.gameObject;
				GetHashedColliders(other.transform.root.name).Add(other.gameObject);

				if (GameVision.IsVisible(other.transform.root.gameObject, parent.Owner.VisionMask)) {
					detected[other.transform.root.name] = target;
					bus.Local(new SensorUpdateEvent<T>(bus, target, true));
				}
			}
		}

		protected virtual void OnTriggerExit (Collider other) {
			if (!initialized) return;

			if (colliders.TryGetValue(other.transform.root.name, out HashSet<GameObject> colliderTable)) {
				colliderTable.Remove(other.gameObject);

				if (colliderTable.Count <= 0) OutOfRange(other.transform.root.name);
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

		protected virtual void OnEntityDestroy (EntityDestroyEvent _event) {
			OutOfRange(_event.Entity.gameObject.name);
		}

		public virtual bool IsDetected (string name) {
			return detected.ContainsKey(name);
		}

		public abstract bool IsDetected (T unit);

		protected virtual void OutOfRange (string key) {
			if (!inRange.ContainsKey(key)) return;

			if (!EntityCache.TryGet(key, out EventAgent targetBus)) return;

			targetBus.RemoveListener<EntityDestroyEvent>(OnEntityDestroy);

			T toRemove = inRange[key];

			if (detected.ContainsKey(key)) {
				detected.Remove(key);
				bus.Local(new SensorUpdateEvent<T>(bus, toRemove, false));
			}
			
			inRange.Remove(key);
			colliders.Remove(key);
		}

		protected virtual HashSet<GameObject> GetHashedColliders (string key) {
			HashSet<GameObject> output = colliders.GetValueOrDefault(key, new HashSet<GameObject>());
			if (!colliders.ContainsKey(key)) colliders[key] = output;
			return output;
		}

		public GameObject GetDetectedCollider (string key) {
			return GetDetectedColliders(key)[0];
		}

		public GameObject[] GetDetectedColliders (string key) {
			if (colliders.TryGetValue(key, out HashSet<GameObject> collider)) {
				return collider.ToArray();
			}

			return null;
		}
	}
}