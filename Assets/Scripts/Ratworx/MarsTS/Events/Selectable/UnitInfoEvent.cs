using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class UnitInfoEvent : SelectableEvent {
		public string Key { get { return Unit.RegistryKey; } }
		public UnitInfoCard Info { get; private set; }

		public UnitInfoEvent (EventAgent _source, ISelectable _unit, UnitInfoCard _infoCard) : base("Info", _source, _unit) {
			Info = _infoCard;
		}
	}
}