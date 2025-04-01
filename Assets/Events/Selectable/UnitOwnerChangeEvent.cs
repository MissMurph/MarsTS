using MarsTS.Teams;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitOwnerChangeEvent : SelectableEvent {

		public Faction NewOwner { get; private set; }

		public UnitOwnerChangeEvent (EventAgent _source, ISelectable _unit, Faction _newOwner) : base("OwnerChange", _source, _unit) {
			NewOwner = _newOwner;
		}
	}
}