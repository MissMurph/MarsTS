using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class SneakEvent : UnitEvent {

		public bool IsSneaking { get; private set; }

		public SneakEvent (EventAgent _source, ISelectable _unit, bool _isSneaking) : base("Sneak", _source, _unit) {
			IsSneaking = _isSneaking;
		}
	}
}