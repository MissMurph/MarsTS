using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class PumpjackOilDetector : MonoBehaviour {

		private EventAgent bus;

		private void Awake () {
			bus = GetComponentInParent<EventAgent>();
		}

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out OilDeposit found)) {
				bus.Local(new PumpjackExploitInitEvent(bus, found));
			}
		}
	}
}