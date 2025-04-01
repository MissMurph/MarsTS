using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class SelectableEvent : AbstractEvent {

		public ISelectable Unit { get; private set; }

		protected SelectableEvent (string name, EventAgent _source, ISelectable _unit) : base("selectable" + name, _source) {
			Unit = _unit;
		}
	}
}