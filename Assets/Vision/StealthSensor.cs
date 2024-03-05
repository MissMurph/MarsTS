using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Vision {

    public class StealthSensor : AbstractSensor<ISelectable> {



		protected override void OnVisionUpdate (VisionUpdateEvent _event) {
			if (_event.Phase == Phase.Pre) return;

			foreach (ISelectable unit in inRange.Values) {
				if (GameVision.IsVisible(unit.GameObject.transform.position, parent.Owner.VisionMask)) {
					EntityCache.TryGet(unit.GameObject.name, out Entity entityComp);
					EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");


				}
			}
		}

		protected virtual void OnOtherEntityVisibleEvent (EntityVisibleEvent _event) {

		}

		protected override void OnTriggerEnter (Collider other) {
			if (!initialized) return;

			if (EntityCache.TryGet(other.transform.root.name, out Entity entityComp)
				&& entityComp.TryGet(out ISelectable target)) {
				EventAgent targetBus = entityComp.Get<EventAgent>("eventAgent");

				targetBus.AddListener<UnitDeathEvent>((_event) => OutOfRange(_event.Unit.GameObject.name));
				targetBus.AddListener<EntityVisibleEvent>(OnOtherEntityVisibleEvent);

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
	}
}