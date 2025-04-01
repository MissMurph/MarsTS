namespace Ratworx.MarsTS.Events.Init {

	public class EventAgentInitEvent : AbstractEvent {

		public EventAgentInitEvent (EventAgent _source) : base("eventAgentInit", _source) {
		}
	}
}