using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Events.Commands {

	public class ProductionEvent : AbstractEvent {

		public ISelectable Producer { get; private set; }
		public ProductionQueue Queue { get; private set; }
		public IProducable CurrentProduction { get; private set; }

		protected ProductionEvent (string name, EventAgent _source, ISelectable _producer, ProductionQueue _queue, IProducable _current) : base("production" + name, _source) {
			Producer = _producer;
			Queue = _queue;
			CurrentProduction = _current;
		}

		public static ProductionEvent Queued (EventAgent _source, ISelectable _producer, ProductionQueue _queue, IProducable _current) {
			return new ProductionEvent("Queued", _source, _producer, _queue, _current);
		}

		public static ProductionEvent Started (EventAgent _source, ISelectable _producer, ProductionQueue _queue, IProducable _current) {
			return new ProductionEvent("Started", _source, _producer, _queue, _current);
		}

		public static ProductionEvent Step (EventAgent _source, ISelectable _producer, ProductionQueue _queue, IProducable _current) {
			return new ProductionEvent("Step", _source, _producer, _queue, _current);
		}

		public static ProductionCompleteEvent Complete (EventAgent _source, GameObject _product, ISelectable _producer, ProductionQueue _queue, IProducable _order) {
			return new ProductionCompleteEvent(_source, _product, _producer, _queue, _order);
		}
	}
}