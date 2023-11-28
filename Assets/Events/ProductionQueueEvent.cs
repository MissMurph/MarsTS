using MarsTS.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ProductionQueueEvent : AbstractEvent {

		public ProductionCommandlet[] Queue { get; private set; }

		public ICommandable Producer { get; private set; }

		public ProductionQueueEvent (EventAgent _source, ProductionCommandlet[] _queue, ICommandable _producer) : base("productionQueued", _source) {
			Queue = _queue;
			Producer = _producer;
		}
	}
}