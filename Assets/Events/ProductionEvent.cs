using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ProductionEvent : AbstractEvent {

		public ISelectable Unit { get; private set; }

		public GameObject Object { get { return Unit.GameObject; } }

		public ICommandable Producer { get; private set; }

		public ProductionEvent (EventAgent _source, ISelectable _unit, ICommandable _producer) : base("unitProduction", _source) {
			Unit = _unit;
			Producer = _producer;
		}
	}
}