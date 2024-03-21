using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class TeamsInitEvent : AbstractEvent {



		public TeamsInitEvent (EventAgent _source) : base("teamsInit", _source) {

		}
	}
}