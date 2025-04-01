using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class PlayerInitEvent : AbstractEvent {



		public PlayerInitEvent (EventAgent _source) : base("playerInit", _source) {
		}
	}
}