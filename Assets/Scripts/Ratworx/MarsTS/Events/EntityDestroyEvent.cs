using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events {

	public class EntityDestroyEvent : AbstractEvent {

		public Entity Entity { get; private set; }

		public EntityDestroyEvent (Entity entity) : base("entityDestroyed") {
			Entity = entity;
		}
	}
}