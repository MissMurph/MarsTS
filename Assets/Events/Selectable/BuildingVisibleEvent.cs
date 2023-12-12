using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class BuildingVisibleEvent : UnitVisibleEvent {

		public bool Visited { get; private set; }

		public BuildingVisibleEvent (EventAgent _source, bool _visible, bool _visited) : base(_source, _visible) {
			Visited = _visited;
		}
	}
}