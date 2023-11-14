using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ProductionEvent : AbstractEvent {

		public ISelectable Unit { get; private set; }

		public GameObject Object {
			get {
				return Unit.GameObject;
			}
		}

		public ProductionEvent (EventAgent _source, ISelectable unit) : base("unitProduction", _source) {
			Unit = unit;
		}
	}
}