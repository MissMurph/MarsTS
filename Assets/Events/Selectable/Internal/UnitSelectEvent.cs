using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitSelectEvent : AbstractEvent {

		public bool Status { get; private set; }

		public UnitSelectEvent (EventAgent _source, bool status) : base("unitSelect", _source) {
			Status = status;
		}
	}
}