using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EventAgentInitEvent : AbstractEvent {

		public EventAgent Agent {
			get {
				return agent;
			}
		}

		private EventAgent agent;

		public EventAgentInitEvent (EventAgent _source) : base("eventAgentInit", _source) {
			agent = _source;
		}
	}
}