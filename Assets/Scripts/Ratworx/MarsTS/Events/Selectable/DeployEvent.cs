using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

    public class DeployEvent : SelectableEvent {

        public bool IsDeployed { get; private set; }

        public DeployEvent (EventAgent _source, ISelectable _unit, bool _isDeployed) : base("Deploy", _source, _unit) {
            IsDeployed = _isDeployed;
        }
    }
}