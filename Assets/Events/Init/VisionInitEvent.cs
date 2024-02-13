using MarsTS.Vision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class VisionInitEvent : AbstractEvent {

		public GameVision Vision { get; private set; }

		public VisionInitEvent (EventAgent _source) : base("visionInit", _source) {
		}
	}
}