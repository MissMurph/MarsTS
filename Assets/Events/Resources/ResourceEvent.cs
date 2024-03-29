using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ResourceEvent : AbstractEvent {

		public string Resource { get; private set; }

		protected ResourceEvent (string name, EventAgent _source, string _resource) : base("resource" + name, _source) {
			Resource = _resource;
		}
	}
}