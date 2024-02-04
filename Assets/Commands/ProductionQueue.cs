using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MarsTS.Commands {

    public class ProductionQueue : CommandQueue {

		//How many steps will occur per second
		[SerializeField]
		protected float productionSpeed;
		protected float stepTime;
		protected float timeToStep;

		protected override void Awake () {
			base.Awake();

			stepTime = 1f / productionSpeed;
			timeToStep = stepTime;
		}

		protected override void Update () {
			if (Current is null && commandQueue.TryDequeue(out Commandlet order)) {
				Current = order;

				order.Callback.AddListener(OrderComplete);
				CommandStartEvent _event = new CommandStartEvent(bus, order, parent);
				order.OnStart(this, _event);

				bus.Local(_event);
				bus.Global(ProductionEvent.Started(bus, parent, this, Current as IProducable));

				return;
			}

			if (Current != null && Current is IProducable production) {
				timeToStep -= Time.deltaTime;

				if (timeToStep <= 0) {
					production.ProductionProgress++;

					if (production.ProductionProgress >= production.ProductionRequired) {
						Current.OnComplete(this, new CommandCompleteEvent(bus, Current, false, parent));
					}
					else bus.Global(ProductionEvent.Step(bus, parent, this, production));

					timeToStep += stepTime;
				}
			}
		}

		public override void Execute (Commandlet order) {
			if (order is not IProducable) {
				Debug.LogWarning("Non Production Command sent to Production Queue! Command ignored");
				return;
			}

			Enqueue(order);
		}

		public override void Enqueue (Commandlet order) {
			if (order is not IProducable) {
				Debug.LogWarning("Non Production Command sent to Production Queue! Command ignored");
				return;
			}

			base.Enqueue(order);

			bus.Global(ProductionEvent.Queued(bus, parent, this, Current as IProducable));
		}

		public override bool CanCommand (string key) {
			if (Current != null && Current.Name == key && Current is UpgradeCommandlet) return false;

			foreach (Commandlet order in Queue) {
				if (order.Name == key && order is UpgradeCommandlet) {
					return false;
				}
			}

			return base.CanCommand(key);
		}
	}
}