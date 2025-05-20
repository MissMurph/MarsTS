using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Selectable;

namespace Ratworx.MarsTS.Events.Commands
{
    public class CooldownEvent : UnitEvent
    {
        public Commandlet Command { get; private set; }
        public float RemainingTime { get; private set; }

        public CooldownEvent(Commandlet command, Entity unit, float remainingTime) : base("cooldown", unit) {
            Command = command;
            RemainingTime = remainingTime;
        }
    }
}