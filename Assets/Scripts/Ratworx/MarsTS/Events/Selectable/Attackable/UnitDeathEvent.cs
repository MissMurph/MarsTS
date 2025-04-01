using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable.Attackable {

	public class UnitDeathEvent : SelectableEvent {

		public UnitDeathEvent (EventAgent _source, ISelectable _unit) : base("Death", _source, _unit) {
		}
	}
}