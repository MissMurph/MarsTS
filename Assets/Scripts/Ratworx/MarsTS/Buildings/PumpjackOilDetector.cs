using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Building;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings {

    public class PumpjackOilDetector : MonoBehaviour {

		private EventAgent bus;

		private void Awake () {
			bus = GetComponentInParent<EventAgent>();
		}

		private void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGetEntityComponent(other.transform.root.name, out OilDeposit found)) {
				bus.Local(new PumpjackExploitInitEvent(bus, found));
			}
		}
	}
}