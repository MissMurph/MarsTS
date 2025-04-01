using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Buildings {

    public class MakeshiftPumpjack : Pumpjack {

		public override bool CanHarvest (string resourceKey, ISelectable unit) {
			return resourceKey == "oil" && unit.UnitType == "roughneck";
		}
	}
}