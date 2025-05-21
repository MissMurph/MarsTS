using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Selectable
{
    public class UnitEvent : AbstractEvent
    {
        public Entity Entity { get; private set; }

        protected UnitEvent(
            string name,
            Entity entity
        ) : base(
            "selectable" + name
        ) {
            Entity = entity;
        }
    }
}