using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MarsTS.Commands {

    public class ProductionQueue : CommandQueue {

		public IProducable[] QueuedProduction {
			get {
				IProducable[] output = new IProducable[commandQueue.Count];
				Commandlet[] originalQueue = commandQueue.ToArray();

				for (int i = 0; i < commandQueue.Count; i++) {
					output[i] = originalQueue[i] as IProducable;
				}

				return output;
			}
		}

		//How many steps will occur per second
		[SerializeField]
		protected float productionSpeed;
		protected float stepTime;
		protected float timeToStep;

		protected override void Awake () {
			base.Awake();

			stepTime = 1f / productionSpeed;
			timeToStep = 0f;
		}

		protected override void Update () {
			if (isServer && Current == null && commandQueue.TryDequeue(out Commandlet order)) {
				Dequeue(order);
				DequeueClientRpc(order.gameObject);

				return;
			}

			if (isServer && Current != null && Current is IProducable production) {
				timeToStep -= Time.deltaTime;

				if (timeToStep <= 0) {
					production.ProductionProgress++;

					if (production.ProductionProgress >= production.ProductionRequired) {
						//CompleteCurrentCommand(false);
						//CompleteCommandClientRpc(false);
					}
					else bus.Global(ProductionEvent.Step(bus, parent, this, production));

					timeToStep += stepTime;
				}
			}
		}

		protected override void Dequeue (Commandlet order) {
			base.Dequeue(order);

			Debug.Log("Dequeueing order!");

			IProducable productionOrder = Current as IProducable;

			productionOrder.OnWork += OnOrderWork;

			bus.Global(ProductionEvent.Started(bus, parent, this, Current as IProducable));
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

			if (!orderSource.CanCommand(order.Command.Name)) return;

			base.Enqueue(order);

			bus.Global(ProductionEvent.Queued(bus, parent, this, Current as IProducable));
		}

		public override bool CanCommand (string key) {
			if (Current != null && Current.Command.Name == key && Current is UpgradeCommandlet) return false;

			foreach (Commandlet order in Queue) {
				if (order.Name == key && order is UpgradeCommandlet) {
					return false;
				}
			}

			return base.CanCommand(key);
		}

		protected override void OnOrderWork (int oldValue, int newValue) {
			IProducable productionOrder = Current as IProducable;

			if (productionOrder.ProductionProgress >= productionOrder.ProductionRequired) {
				productionOrder.OnWork -= OnOrderWork;
				Debug.Log("Production order is complete!");
				bus.Global(new ProductionCompleteEvent(bus, productionOrder.Product, parent, this, productionOrder));
				Current.OnComplete(this, new CommandCompleteEvent(bus, Current, false, parent));
			} 
			else bus.Global(ProductionEvent.Step(bus, parent, this, productionOrder));
		}
	}
}