using Ratworx.MarsTS.Vision;

namespace Ratworx.MarsTS.Events.Init {

	public class VisionInitEvent : AbstractEvent {

		public GameVision Vision { get; private set; }

		public VisionInitEvent (EventAgent _source) : base("visionInit", _source) {
		}
	}
}