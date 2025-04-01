using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class MakeshiftPumpjack : Pumpjack {

		public override bool CanHarvest (string resourceKey, ISelectable unit) {
			return resourceKey == "oil" && unit.UnitType == "roughneck";
		}
	}
}