using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Init {

	public class EntityInitEvent : AbstractEvent {

		public Entity ParentEntity { get; private set; }

		public EntityInitEvent (Entity _parentEntity, EventAgent _source) : base("entityInit", _source) {
			ParentEntity = _parentEntity;
		}
	}
}