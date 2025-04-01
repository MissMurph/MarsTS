using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings.Upgrades {

	public class CamoUpgrade : MonoBehaviour {

		private void Start () {
			ISelectable parent = GetComponentInParent<ISelectable>();
			EventAgent bus = GetComponentInParent<EventAgent>();
			GetComponentInParent<EventAgent>().Local(new SneakEvent(bus, parent, true));
		}
	}
}