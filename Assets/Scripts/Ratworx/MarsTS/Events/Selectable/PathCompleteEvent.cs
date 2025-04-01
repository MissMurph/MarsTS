namespace Ratworx.MarsTS.Events.Selectable {

	public class PathCompleteEvent : AbstractEvent {

		public bool Complete { get; private set; }

		public PathCompleteEvent (EventAgent _source, bool _complete) : base("pathComplete", _source) {
			Complete = _complete;
		}
	}
}