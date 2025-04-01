using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Events.Commands {

    public class ProductionCompleteEvent : ProductionEvent {

		public GameObject Object { get; private set; }

		public ProductionCompleteEvent (EventAgent _source, GameObject _product, ISelectable _producer, ProductionQueue _queue, IProducable _order) 
			: base("Complete", _source, _producer, _queue, _order) {
			Object = _product;
		}
	}
}