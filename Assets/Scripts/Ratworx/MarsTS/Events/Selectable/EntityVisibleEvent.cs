using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class EntityVisibleEvent : SelectableEvent {

		public bool Visible { get; set; }
		public string UnitName { get { return Unit.GameObject.name; } }

		public EntityVisibleEvent (EventAgent _source, ISelectable _unit, bool _visible) : base("Visible", _source, _unit) {
			Visible = _visible;
		}
	}
}