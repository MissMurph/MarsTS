using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityVisibleCheckEvent : SelectableEvent {

		public int VisibleTo { get; set; }

		public EntityVisibleCheckEvent (EventAgent _source, ISelectable _unit, int _visibleTo) : base("visibleCheck", _source, _unit) {
			VisibleTo = _visibleTo;
		}
	}
}