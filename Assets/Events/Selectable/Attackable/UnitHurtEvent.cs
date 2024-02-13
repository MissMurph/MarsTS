using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitHurtEvent : SelectableEvent {

		public IAttackable Targetable { get; private set; }

		public UnitHurtEvent (EventAgent _source, IAttackable _unit) : base("Hurt", _source, _unit as ISelectable) {
			Targetable = _unit;
		}
	}
}