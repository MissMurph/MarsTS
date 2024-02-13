using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class HarvestSensor : AbstractSensor<IHarvestable> {

		public override bool IsDetected (IHarvestable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}
	}
}