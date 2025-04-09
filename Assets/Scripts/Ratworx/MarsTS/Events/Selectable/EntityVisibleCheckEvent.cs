using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Vision;

namespace Ratworx.MarsTS.Events.Selectable
{
    public class EntityVisibleCheckEvent : UnitEvent
    {
        public int VisibleTo { get; set; }

        public EntityVisibleCheckEvent(
            UnitVision unitVision,
            int visibleTo
        ) : base(
            "visibleCheck",
            unitVision.Entity
        ) {
            VisibleTo = visibleTo;
        }
    }
}