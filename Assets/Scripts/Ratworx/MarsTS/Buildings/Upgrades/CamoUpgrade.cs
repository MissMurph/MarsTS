using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

	public class CamoUpgrade : MonoBehaviour {

		private void Start () {
			ISelectable parent = GetComponentInParent<ISelectable>();
			EventAgent bus = GetComponentInParent<EventAgent>();
			GetComponentInParent<EventAgent>().Local(new SneakEvent(bus, parent, true));
		}
	}
}