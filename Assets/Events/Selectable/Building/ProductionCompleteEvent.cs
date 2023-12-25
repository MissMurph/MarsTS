using MarsTS.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

    public class ProductionCompleteEvent : ProductionEvent {

		public GameObject Object { get; private set; }

		public ProductionCompleteEvent (EventAgent _source, GameObject _prefab, ICommandable _producer) : base("Complete", _source, _producer) {
			Object = _prefab;
		}
	}
}