using MarsTS.UI;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitInfoEvent : AbstractEvent {

		public ISelectable Unit { get; private set; }
		public string Key { get { return Unit.RegistryKey; } }

		public UnitInfoCard Info { get; private set; }

		public UnitInfoEvent (EventAgent _source, ISelectable _unit, UnitInfoCard _infoCard) : base("unitInfo", _source) {
			Unit = _unit;
			Info = _infoCard;
		}
	}
}