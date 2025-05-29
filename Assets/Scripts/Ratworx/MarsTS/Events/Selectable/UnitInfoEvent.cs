using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class UnitInfoEvent : UnitEvent {
		public UnitInfoCard Info { get; private set; }

		public UnitInfoEvent (Entity unit, UnitInfoCard infoCard) : base("Info", unit) {
			Info = infoCard;
		}
	}
}