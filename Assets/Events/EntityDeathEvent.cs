using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityDeathEvent : AbstractEvent {

		public ISelectable Unit { get { return unit; } }

		private ISelectable unit;
		
		public EntityDeathEvent (EventAgent _source, ISelectable _unit) : base("entityDeath", _source) {
			unit = _unit;
		}
	}
}