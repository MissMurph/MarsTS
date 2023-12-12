using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitProducedEvent : ProductionEvent {

		public ISelectable Unit { get; private set; }

		public GameObject Object { get { return Unit.GameObject; } }

		public UnitProducedEvent (EventAgent _source, ISelectable _unit, ICommandable _producer) : base("Complete", _source, _producer) {
			Unit = _unit;
		}
	}
}