namespace Ratworx.MarsTS.Events.Selectable {

	public class PathCompleteEvent : AbstractEvent {

		public bool Complete { get; private set; }

		public PathCompleteEvent (bool _complete) : base("pathComplete") {
			Complete = _complete;
		}
	}
}