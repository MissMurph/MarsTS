using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

    public class ProductionCompleteEvent : ProductionEvent {

		public GameObject Object { get; private set; }

		public ProductionCompleteEvent (EventAgent _source, GameObject _product, ISelectable _producer, ProductionQueue _queue, IProducable _order) 
			: base("Complete", _source, _producer, _queue, _order) {
			Object = _product;
		}
	}
}