using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityHurtEvent : AbstractEvent {

		public ISelectable Unit {
			get {
				return unit;
			}
		}

		private ISelectable unit;

		public EntityHurtEvent (EventAgent _source, ISelectable _unit) : base("entityHurt", _source) {
			unit = _unit;
		}
	}
}