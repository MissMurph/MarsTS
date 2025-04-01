using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class EntityVisibleCheckEvent : SelectableEvent {

		public int VisibleTo { get; set; }

		public EntityVisibleCheckEvent (EventAgent _source, ISelectable _unit, int _visibleTo) : base("visibleCheck", _source, _unit) {
			VisibleTo = _visibleTo;
		}
	}
}