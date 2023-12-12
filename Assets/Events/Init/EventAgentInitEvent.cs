using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EventAgentInitEvent : AbstractEvent {

		public EventAgentInitEvent (EventAgent _source) : base("eventAgentInit", _source) {
		}
	}
}