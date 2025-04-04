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

			_event.VisibleTo |= Parent.Owner.VisionMask;
		}

		protected override void OnTriggerEnter (Collider other) {
			if (!IsInitialized) return;

			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp)
				&& entityComp.TryGet(out ISelectable target)) {
				EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

				targetBus.AddListener<UnitDeathEvent>(OnUnitDeath);
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

			targetBus.RemoveListener<UnitDeathEvent>(OnUnitDeath);
			targetBus.RemoveListener<EntityVisibleCheckEvent>(OnOtherEntityVisibleEvent);

			ISelectable toRemove = inRange[key];

			if (detected.ContainsKey(key)) {
				detected.Remove(key);
				Bus.Local(new SensorUpdateEvent<ISelectable>(Bus, toRemove, false));
			}

			inRange.Remove(key);
		}
	}
}