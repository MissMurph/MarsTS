using MarsTS.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class BuildingConstructStepEvent : AbstractEvent {

		public Building Building {
			get {
				return building;
			}
		}

		private Building building;

		public BuildingConstructStepEvent (EventAgent _source, Building _building) : base("buildingConstructStep", _source) {
			building = _building;
		}
	}
}