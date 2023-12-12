using MarsTS.Vision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class VisionUpdateEvent : AbstractEvent {
		
		public GameVision Vision { get; private set; }

		public VisionUpdateEvent (EventAgent _source) : base("visionUpdate", _source) {
		}
	}
} 