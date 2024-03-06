using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

    public class StealthSensor : AbstractSensor<ISelectable> {

		protected virtual void OnOtherEntityVisibleEvent (EntityVisibleCheckEvent _event) {
			if (_event.Phase == Phase.Pre) return;

			_event.VisibleTo |= parent.Owner.VisionMask;
		}

		protected override void OnTriggerEnter (Collider other) {
			if (!initialized) return;

			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp)
				&& entityComp.TryGet(out ISelectable target)) {
				EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

				targetBus.AddListener<UnitDeathEvent>((_event) => OutOfRange(_event.Unit.GameObject.name));
				targetBus.AddListener<EntityVisibleCheckEvent>(OnOtherEntityVisibleEvent);

				inRange[other.transform.root.name] = target;

				/*if (GameVision.IsVisible(other.transform.root.gameObject, parent.Owner.VisionMask)) {
					detected[other.transform.root.name] = target;
					bus.Local(new SensorUpdateEvent<ISelectable>(bus, target, true));
				}*/
			}
		}

		public override bool IsDetected (ISelectable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}

		protected override void OutOfRange (string name) {
			if (!inRange.ContainsKey(name)) return;

			EntityCache.TryGet(name, out Entity entityComp);

			EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

			targetBus.RemoveListener<UnitDeathEvent>((_event) => OutOfRange(_event.Unit.GameObject.name));
			targetBus.RemoveListener<EntityVisibleCheckEvent>(OnOtherEntityVisibleEvent);

			ISelectable toRemove = inRange[name];

			if (detected.ContainsKey(name)) {
				detected.Remove(name);
				bus.Local(new SensorUpdateEvent<ISelectable>(bus, toRemove, false));
			}

			inRange.Remove(name);
		}
	}
}