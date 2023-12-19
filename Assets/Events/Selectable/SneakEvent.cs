using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class SneakEvent : SelectableEvent {

		public bool IsSneaking { get; private set; }

		public SneakEvent (EventAgent _source, ISelectable _unit, bool _isSneaking) : base("Sneak", _source, _unit) {
			IsSneaking = _isSneaking;
		}
	}
}