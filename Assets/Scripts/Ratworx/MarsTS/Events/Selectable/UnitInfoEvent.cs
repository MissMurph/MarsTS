using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class UnitInfoEvent : UnitEvent {
		public UnitInfoCard Info { get; private set; }
		public ISelectable Selectable { get; private set; }
		public UnitInfoEvent (ISelectable unit, UnitInfoCard infoCard) : base("Info", unit.Entity) {
			Info = infoCard;
			Selectable = unit;
		}
	}
}