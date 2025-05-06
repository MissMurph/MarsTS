using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Vision;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Sensors
{
    [RequireComponent(typeof(Collider))]
    public abstract class AbstractSensor<T> : MonoBehaviour where T : IUnitInterface
    {
        /// <remarks><c>bool</c> value is set to true if the unit was detected, false if it's no longer detected.</remarks>
        public event Action<T, bool> OnUnitDetected;
        /// <remarks><c>bool</c> value is set to true if the unit is in range, false if it's no longer in range.</remarks>
        public event Action<T, bool> OnUnitInRange;
        
        public float Range => _sensorCollider.radius;

        public List<T> Detected => detected.Values.ToList();

        public List<T> InRange => inRange.Values.ToList();

        public List<GameObject> InRangeColliders
        {
            get
            {
                var output = new List<GameObject>();

                foreach (HashSet<GameObject> table in _detectedColliders.Values)
                {
                    output.AddRange(table);
                }

                return output;
            }
        }

        private SphereCollider _sensorCollider;

        protected readonly Dictionary<string, T> inRange = new Dictionary<string, T>();
        protected readonly Dictionary<string, T> detected = new Dictionary<string, T>();

        private readonly Dictionary<string, HashSet<GameObject>> _detectedColliders = new Dictionary<string, HashSet<GameObject>>();
        private readonly List<Collider> _queuedColliders = new List<Collider>();

        protected UnitOwnership Ownership;
        protected EventAgent Bus;
        protected bool IsInitialized;

        private Entity _parentEntity;

        protected virtual void Awake() {
            _sensorCollider = GetComponent<SphereCollider>();
            _sensorCollider.enabled = false;
            Bus = GetComponentInParent<EventAgent>();
            Ownership = GetComponentInParent<UnitOwnership>();
            _parentEntity = GetComponentInParent<Entity>();

            foreach (Collider colliderToIgnore in transform.GetComponentsInChildren<Collider>()) {
                Physics.IgnoreCollision(_sensorCollider, colliderToIgnore, true);
            }

            _parentEntity.OnEntityInit += OnEntityInit;
        }

        protected virtual void Start() {
            EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
        }

        private void OnDisable() {
            _detectedColliders.Clear();
            inRange.Clear();
            detected.Clear();
        }

        protected void Update() {
            if (!IsInitialized 
                || Ownership == null 
                || Ownership.Owner == null 
                || _queuedColliders.Count <= 0
            ) return;

            foreach (var collision in _queuedColliders)
            {
                OnTriggerEnter(collision);
            }
        }

        private void OnEntityInit(Phase phase) {
            if (phase == Phase.Pre) 
                return;
            
            IsInitialized = true;
            _sensorCollider.enabled = true;
        }

        protected virtual void OnTriggerEnter(Collider other) {
            if (other.transform.name == transform.name) return;
            
            if (!IsInitialized 
                || Ownership == null 
                || Ownership.Owner == null
            ) {
                _queuedColliders.Add(other);
                return;
            }
            
            if (EntityCache.TryGetEntity(other.transform.name, out Entity entityComp)
                && entityComp.TryGetEntityComponent(out T target))
            {
                EventAgent targetBus = entityComp.GetEntityComponent<EventAgent>("eventAgent");
                targetBus.AddListener<UnitDeathEvent>(OnUnitDeath);

                inRange[other.transform.name] = target;
                GetHashedColliders(other.transform.name).Add(other.gameObject);
                OnUnitInRange?.Invoke(target, true);

                if (GameVision.IsVisible(other.transform.gameObject, Ownership.Owner.VisionMask)) {
                    detected[other.transform.name] = target;
                    Bus.PostLocal(new SensorUpdateEvent<T>(Bus, target, true));
                    OnUnitDetected?.Invoke(target, true);
                }
            }

            _queuedColliders.Remove(other);
        }

        protected virtual void OnTriggerExit(Collider other) {
            if (!IsInitialized) return;

            if (!_detectedColliders.TryGetValue(other.transform.name, out HashSet<GameObject> colliderTable)) 
                return;
            
            colliderTable.Remove(other.gameObject);

            if (colliderTable.Count <= 0)
                OutOfRange(other.transform.name);
        }

        protected virtual void OnVisionUpdate(VisionUpdateEvent evnt) {
            foreach (KeyValuePair<string, T> inRangeUnit in inRange) {
                if (GameVision.IsVisible(inRangeUnit.Key, Ownership.Owner.VisionMask)) {
                    detected[inRangeUnit.Key] = inRange[inRangeUnit.Key];
                    Bus.PostLocal(new SensorUpdateEvent<T>(Bus, detected[inRangeUnit.Key], true));
                }
                else if (detected.ContainsKey(inRangeUnit.Key)) {
                    T toRemove = detected[inRangeUnit.Key];
                    detected.Remove(inRangeUnit.Key);
                    Bus.PostLocal(new SensorUpdateEvent<T>(Bus, toRemove, false));
                    OnUnitDetected?.Invoke(toRemove, false);
                }
            }
        }

        protected void OnUnitDeath(UnitDeathEvent evnt) => OutOfRange(evnt.Entity.gameObject.name);

        public virtual bool IsDetected(string name) => detected.ContainsKey(name);

        public virtual bool IsDetected(T unit) => IsDetected(unit.GameObject.name);

        protected virtual void OutOfRange(string key) {
            if (!inRange.ContainsKey(key)
                || !EntityCache.TryGetEntityComponent(key, out EventAgent targetBus)) return;

            targetBus.RemoveListener<UnitDeathEvent>(OnUnitDeath);

            T toRemove = inRange[key];

            if (detected.ContainsKey(key)) {
                detected.Remove(key);
                Bus.PostLocal(new SensorUpdateEvent<T>(Bus, toRemove, false));
                OnUnitDetected?.Invoke(toRemove, false);
            }

            inRange.Remove(key);
            _detectedColliders.Remove(key);
            OnUnitInRange?.Invoke(toRemove, false);
        }

        private HashSet<GameObject> GetHashedColliders(string key) {
            HashSet<GameObject> output = _detectedColliders.GetValueOrDefault(key, new HashSet<GameObject>());
            _detectedColliders.TryAdd(key, output);
            return output;
        }

        public GameObject GetDetectedCollider(string key) => GetDetectedColliders(key)[0];

        private GameObject[] GetDetectedColliders(string key)
            => _detectedColliders.TryGetValue(key, out HashSet<GameObject> detectedColliders)
                ? detectedColliders.ToArray()
                : null;
    }
}