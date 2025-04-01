using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class UnitOwnerChangeEvent : SelectableEvent {

		public Faction NewOwner { get; private set; }

		public UnitOwnerChangeEvent (EventAgent _source, ISelectable _unit, Faction _newOwner) : base("OwnerChange", _source, _unit) {
			NewOwner = _newOwner;
		}
	}
}