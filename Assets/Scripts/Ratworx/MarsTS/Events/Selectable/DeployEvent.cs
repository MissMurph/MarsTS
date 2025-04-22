using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

    public class DeployEvent : UnitEvent {

        public bool IsDeployed { get; private set; }

        public DeployEvent (Entity unit, bool isDeployed) : base("Deploy", unit) {
            IsDeployed = isDeployed;
        }
    }
}