using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class StealthSensor : AbstractSensor<ISelectable> {

		public bool Detecting { get; set; }

		protected virtual void OnOtherEntityVisibleEvent (EntityVisibleCheckEvent _event) {
			if (!Detecting || _event.Phase == Phase.Pre) return;

			_event.VisibleTo |= parent.Owner.VisionMask;
		}

		protected override void OnTriggerEnter (Collider other) {
			if (!initialized) return;

			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp)
				&& entityComp.TryGet(out ISelectable target)) {
				EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

				targetBus.AddListener<EntityDestroyEvent>(OnEntityDestroy);
				targetBus.AddListener<EntityVisibleCheckEvent>(OnOtherEntityVisibleEvent);

				inRange[other.transform.root.name] = target;
			}
		}

		public override bool IsDetected (ISelectable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}

		protected override void OutOfRange (string key) {
			if (!inRange.ContainsKey(key)) return;

			EntityCache.TryGet(key, out EventAgent targetBus);

			targetBus.RemoveListener<EntityDestroyEvent>(OnEntityDestroy);
			targetBus.RemoveListener<EntityVisibleCheckEvent>(OnOtherEntityVisibleEvent);

			ISelectable toRemove = inRange[key];

			if (detected.ContainsKey(key)) {
				detected.Remove(key);
				bus.Local(new SensorUpdateEvent<ISelectable>(bus, toRemove, false));
			}

			inRange.Remove(key);
		}
	}
}