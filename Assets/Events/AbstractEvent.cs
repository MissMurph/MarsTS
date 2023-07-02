using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class AbstractEvent {
		public string Name { get; private set; }
		public Phase Phase { get; set; }
		public bool Canceled { get; set; }

		public AbstractEvent (string name) {
			Name = name;
			Phase = Phase.Pre;
			Canceled = false;
		}
	}
}