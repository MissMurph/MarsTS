using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class PathCompleteEvent : AbstractEvent {

		public bool Complete { get; private set; }

		public PathCompleteEvent (EventAgent _source, bool _complete) : base("pathComplete", _source) {
			Complete = _complete;
		}
	}
}