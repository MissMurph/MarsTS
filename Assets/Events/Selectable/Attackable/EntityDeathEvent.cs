using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityDeathEvent : SelectableEvent {

		public EntityDeathEvent (EventAgent _source, ISelectable _unit) : base("Death", _source, _unit) {
		}
	}
}