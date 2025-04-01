using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class SelectableEvent : AbstractEvent {

		public ISelectable Unit { get; private set; }

		protected SelectableEvent (string name, EventAgent _source, ISelectable _unit) : base("selectable" + name, _source) {
			Unit = _unit;
		}
	}
}