namespace Ratworx.MarsTS.Events.Player {

	public class PlayerInitEvent : AbstractEvent {



		public PlayerInitEvent (EventAgent _source) : base("playerInit", _source) {
		}
	}
}