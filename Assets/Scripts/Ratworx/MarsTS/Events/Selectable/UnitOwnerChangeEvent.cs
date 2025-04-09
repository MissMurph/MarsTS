using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable
{
    public class UnitOwnerChangeEvent : UnitEvent
    {
        public Faction NewOwner { get; private set; }

        public UnitOwnerChangeEvent(
            UnitOwnership unitOwnership,
            Faction newOwner
        ) : base(
            "OwnerChange",
            unitOwnership.Entity
        ) {
            NewOwner = newOwner;
        }
    }
}