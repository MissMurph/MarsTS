using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class DepositSensor : AbstractSensor<IDepositable> {
		
		public override bool IsDetected (IDepositable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}
	}
}