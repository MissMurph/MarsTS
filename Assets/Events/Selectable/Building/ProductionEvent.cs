using MarsTS.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ProductionEvent : AbstractEvent {

		public ICommandable Producer { get; private set; }
		public ProductionCommandlet CurrentProduction { get; private set; }
		public ProductionCommandlet[] ProductionQueue { get; private set; }

		protected ProductionEvent (string name, EventAgent _source, ICommandable _producer) : base("production" + name, _source) {
			Producer = _producer;

			CurrentProduction = Producer.CurrentCommand as ProductionCommandlet;
			ProductionQueue = Producer.CommandQueue as ProductionCommandlet[];
		}

		public static ProductionEvent Queued (EventAgent _source, ICommandable _producer) {
			return new ProductionEvent("Queued", _source, _producer);
		}

		public static ProductionEvent Started (EventAgent _source, ICommandable _producer) {
			return new ProductionEvent("Started", _source, _producer);
		}

		public static ProductionEvent Step (EventAgent _source, ICommandable _producer) {
			return new ProductionEvent("Step", _source, _producer);
		}
	}
}