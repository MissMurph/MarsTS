using Ratworx.MarsTS.Vision;

namespace Ratworx.MarsTS.Events.Player {

	public class VisionUpdateEvent : AbstractEvent {
		
		public GameVision Vision { get; private set; }

		public VisionUpdateEvent (EventAgent _source) : base("visionUpdate", _source) {
		}
	}
} 