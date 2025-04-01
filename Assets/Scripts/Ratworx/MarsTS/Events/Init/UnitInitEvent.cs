using MarsTS.Units;

namespace MarsTS.Events
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