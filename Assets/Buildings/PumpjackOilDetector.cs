using MarsTS.Entities;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class PumpjackOilDetector : MonoBehaviour {

        public OilDeposit hitExploited;

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out OilDeposit found)) {
				hitExploited = found;
			}
		}
	}
}