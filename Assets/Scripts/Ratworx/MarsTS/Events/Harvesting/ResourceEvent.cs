namespace Ratworx.MarsTS.Events.Harvesting {

	public class ResourceEvent : AbstractEvent {

		public string Resource { get; private set; }

		protected ResourceEvent (string name, string resource) : base("resource" + name) {
			Resource = resource;
		}
	}
}