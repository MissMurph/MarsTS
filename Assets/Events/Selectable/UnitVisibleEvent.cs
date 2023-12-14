using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitVisibleEvent : SelectableEvent {

		public bool Visible { get; private set; }
		public string UnitName { get { return Unit.GameObject.name; } }

		public UnitVisibleEvent (EventAgent _source, ISelectable _unit, bool _visible) : base("Visible", _source, _unit) {
			Visible = _visible;
		}
	}
}