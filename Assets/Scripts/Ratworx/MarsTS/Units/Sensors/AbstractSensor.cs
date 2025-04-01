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
    public abstract class AbstractSensor<T> : MonoBehaviour where T : IUnit
    {
        public float Range => SensorCollider.radius;

        public List<T> Detected => detected.Values.ToList();

        public List<T> InRange => inRange.Values.ToList();

        public List<GameObject> InRangeColliders
        {
            get
            {
                var output = new List<GameObject>();

                foreach (HashSet<GameObject> table in DetectedColliders.Values)
                {
                    output.AddRange(table);
                }

                return output;
            }
        }

        protected SphereCollider SensorCollider;

        protected Dictionary<string, T> inRange = new Dictionary<string, T>();

        protected Dictionary<string, T> detected = new Dictionary<string, T>();

        protected Dictionary<string, HashSet<GameObject>> DetectedColliders = new Dictionary<string, HashSet<GameObject>>();

        protected List<Collider> QueuedColliders = new List<Collider>();

        protected ISelectable Parent;

        protected EventAgent Bus;

        protected Entity _parentEntity;

        protected bool IsInitialized;

        protected virtual void Awake()
        {
            SensorCollider = GetComponent<SphereCollider>();
            SensorCollider.enabled = false;
            Bus = GetComponentInParent<EventAgent>();
            Parent = GetComponentInParent<ISelectable>();
            _parentEntity = GetComponentInParent<Entity>();

            foreach (Collider colliderToIgnore in transform.root.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(SensorCollider, colliderToIgnore, true);
            }

            //Bus.AddListener<EntityInitEvent>(OnEntityInit);
            _parentEntity.OnEntityInit += OnEntityInit;
        }

        protected virtual void Start()
        {
            EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
        }

        protected void Update()
        {
            if (!IsInitialized 
                || Parent == null 
                || Parent.Owner == null 
                || QueuedColliders.Count <= 0
            ) return;

            foreach (var collision in QueuedColliders)
            {
                OnTriggerEnter(collision);
            }
        }

        protected void OnEntityInit(Phase phase)
        {
            if (phase == Phase.Pre) 
                return;
            
            IsInitialized = true;
            SensorCollider.enabled = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!IsInitialized || Parent == null || Parent.Owner == null)
            {
                QueuedColliders.Add(other);
                return;
            }

            if (other.transform.root.name == transform.root.name) return;
            
            if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp)
                && entityComp.TryGet(out T target))
            {
                EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

                targetBus.AddListener<UnitDeathEvent>(OnUnitDeath);

                inRange[other.transform.root.name] = target;
                //colliders[other.transform.root.name] = other.gameObject;
                GetHashedColliders(other.transform.root.name).Add(other.gameObject);

                if (GameVision.IsVisible(other.transform.root.gameObject, Parent.Owner.VisionMask))
                {
                    detected[other.transform.root.name] = target;
                    Bus.Local(new SensorUpdateEvent<T>(Bus, target, true));
                }
            }

            QueuedColliders.Remove(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!IsInitialized) return;

            if (DetectedColliders.TryGetValue(other.transform.root.name, out HashSet<GameObject> colliderTable))
            {
                colliderTable.Remove(other.gameObject);

                if (colliderTable.Count <= 0) OutOfRange(other.transform.root.name);
            }
        }

        protected virtual void OnVisionUpdate(VisionUpdateEvent _event)
        {
            foreach (KeyValuePair<string, T> inRangeUnit in inRange)
            {
                if (GameVision.IsVisible(inRangeUnit.Key, Parent.Owner.VisionMask))
                {
                    detected[inRangeUnit.Key] = inRange[inRangeUnit.Key];
                    Bus.Local(new SensorUpdateEvent<T>(Bus, detected[inRangeUnit.Key], true));
                }
                else if (detected.ContainsKey(inRangeUnit.Key))
                {
                    T toRemove = detected[inRangeUnit.Key];
                    detected.Remove(inRangeUnit.Key);
                    Bus.Local(new SensorUpdateEvent<T>(Bus, toRemove, false));
                }
            }
        }

        protected virtual void OnUnitDeath(UnitDeathEvent _event)
        {
            OutOfRange(_event.Unit.GameObject.name);
        }

        public virtual bool IsDetected(string name) => detected.ContainsKey(name);

        public abstract bool IsDetected(T unit);

        protected virtual void OutOfRange(string key)
        {
            if (!inRange.ContainsKey(key)) return;

            if (!EntityCache.TryGet(key, out EventAgent targetBus)) return;

            targetBus.RemoveListener<UnitDeathEvent>(OnUnitDeath);

            T toRemove = inRange[key];

            if (detected.ContainsKey(key))
            {
                detected.Remove(key);
                Bus.Local(new SensorUpdateEvent<T>(Bus, toRemove, false));
            }

            inRange.Remove(key);
            DetectedColliders.Remove(key);
        }

        protected virtual HashSet<GameObject> GetHashedColliders(string key)
        {
            HashSet<GameObject> output = DetectedColliders.GetValueOrDefault(key, new HashSet<GameObject>());
            if (!DetectedColliders.ContainsKey(key)) DetectedColliders[key] = output;
            return output;
        }

        public GameObject GetDetectedCollider(string key) => GetDetectedColliders(key)[0];

        public GameObject[] GetDetectedColliders(string key)
        {
            if (DetectedColliders.TryGetValue(key, out HashSet<GameObject> collider)) return collider.ToArray();

            return null;
        }
    }
}