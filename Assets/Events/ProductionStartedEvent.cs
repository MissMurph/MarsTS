using MarsTS.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ProductionStartedEvent : AbstractEvent {

		public ProductionCommandlet Order { get; private set; }

		public ProductionCommandlet[] Queue { get; private set; }

		public ICommandable Producer { get; private set; }

		public ProductionStartedEvent (EventAgent _source, ProductionCommandlet _order, ProductionCommandlet[] _queue, ICommandable _producer) : base("productionStarted", _source) {
			Order = _order;
			Queue = _queue;
			Producer = _producer;
		}
	}
}