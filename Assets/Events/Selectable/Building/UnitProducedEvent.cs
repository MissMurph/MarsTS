using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitProducedEvent : ProductionCompleteEvent {

		public ISelectable Unit { get; private set; }

		public UnitProducedEvent (EventAgent _source, ISelectable _unit, ICommandable _producer) : base(_source, _unit.GameObject, _producer) {
			Unit = _unit;
		}
	}
}