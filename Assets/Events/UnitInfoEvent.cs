using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class UnitInfoEvent : AbstractEvent {

		public ISelectable Unit { get; private set; }
		public string Key { get { return Unit.RegistryKey; } }
		public GameObject InfoCard { get; private set; }

		public UnitInfoEvent (EventAgent _source, ISelectable _unit, GameObject _infoCard) : base("unitInfo", _source) {
			Unit = _unit;
			InfoCard = _infoCard;
		}
	}
}