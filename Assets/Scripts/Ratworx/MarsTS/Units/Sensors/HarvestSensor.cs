using Ratworx.MarsTS.WorldObject;

namespace Ratworx.MarsTS.Units.Sensors {

    public class HarvestSensor : AbstractSensor<IHarvestable> {

		public override bool IsDetected (IHarvestable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}
	}
}