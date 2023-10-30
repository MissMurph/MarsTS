using MarsTS.Buildings;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units.Commands {

	public class PlaceBuilding : Command<ISelectable> {

		public override string Name { get { return "construct/" + building.BuildingType; } }

		[SerializeField]
		private Building building;

		public override void StartSelection () {
			
		}
	}
}