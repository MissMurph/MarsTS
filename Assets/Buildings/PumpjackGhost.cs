using MarsTS.Entities;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class PumpjackGhost : BuildingGhost {

		

		public override bool Legal {
			get {
				foreach (Collider other in collisions) {
					if (EntityCache.TryGet(other.transform.root.name, out ResourceDeposit comp) && comp is OilDeposit) {
						return true;
					}
				}

				return false;
			}
		}
	}
}