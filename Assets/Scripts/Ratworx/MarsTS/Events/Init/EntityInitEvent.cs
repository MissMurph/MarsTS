using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Init {

	public class EntityInitEvent : AbstractEvent {

		public Entity Entity { get; private set; }

		public EntityInitEvent (Entity entity) : base("entityInit") {
			Entity = entity;
		}
	}
}