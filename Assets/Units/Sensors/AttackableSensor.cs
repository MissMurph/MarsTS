using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class AttackableSensor : AbstractSensor<IAttackable> {

		public override bool IsDetected (IAttackable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}
	}
}