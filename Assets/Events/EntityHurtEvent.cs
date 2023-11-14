using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class EntityHurtEvent : AbstractEvent {

		public IAttackable Unit {
			get {
				return unit;
			}
		}

		private IAttackable unit;

		public EntityHurtEvent (EventAgent _source, IAttackable _unit) : base("entityHurt", _source) {
			unit = _unit;
		}
	}
}