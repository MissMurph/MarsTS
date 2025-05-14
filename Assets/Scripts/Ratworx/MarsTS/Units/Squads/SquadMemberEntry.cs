using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Units.Infantry;

namespace Ratworx.MarsTS.Units.Squads
{
    public class SquadMemberEntry
    {
        public int InstanceId;
        public InfantryMember Membership;
        public Entity Entity;
        public EventAgent EventAgent;
        public UnitOwnership Ownership;
        public UnitSelection Selection;
        public CommandQueue CommandQueue;
    }
}