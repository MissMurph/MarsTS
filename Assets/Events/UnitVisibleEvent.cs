using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitVisibleEvent : AbstractEvent {

		public bool Visible { get; private set; }

		public UnitVisibleEvent (EventAgent _source, bool _visible) : base("unitVision", _source) {
			Visible = _visible;
		}
	}
}