using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Init
{
    public class UnitInitEvent : AbstractEvent
    {
        public ISelectable Unit { get; private set; }

        public UnitInitEvent(ISelectable unit, EventAgent source) 
            : base("unitInit", source)
        {
            Unit = unit;
        }
    }
}