namespace Ratworx.MarsTS.Events {

	public class AbstractEvent {
		public string Name { get; private set; }

		public EventAgent Source {
			get {
				return source;
			}
			set {
				if (source != null) source = value;
			}
		}

		private EventAgent source = null;

		public Phase Phase { get; set; }

		public bool Canceled { get; set; }

		public AbstractEvent (string name, EventAgent _source) {
			Name = name;
			Phase = Phase.Pre;
			Canceled = false;
			source = _source;
		}
	}
}