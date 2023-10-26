using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class AbstractEvent {
		public string Name { get; private set; }

		public Agent Source {
			get {
				return source;
			}
			set {
				if (source != null) source = value;
			}
		}

		private Agent source = null;

		public Phase Phase { get; set; }

		public bool Canceled { get; set; }

		public AbstractEvent (string name, Agent _source) {
			Name = name;
			Phase = Phase.Pre;
			Canceled = false;
			source = _source;
		}
	}
}