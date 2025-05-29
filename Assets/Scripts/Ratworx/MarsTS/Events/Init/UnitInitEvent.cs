using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Selectable;

namespace Ratworx.MarsTS.Events.Init
{
    // TODO: stimky
    public class UnitInitEvent : UnitEvent
    {
        public UnitInitEvent(Entity unit)
            : base("unitInit", unit) { }
    }
}