using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

    public class UnitHoverEvent : AbstractEvent {

		public bool Status { get; private set; }

		public UnitHoverEvent (EventAgent _source, bool status) : base("unitHover", _source) {
			Status = status;
		}
	}
}