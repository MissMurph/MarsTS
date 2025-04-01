using Ratworx.MarsTS.Buildings;

namespace Ratworx.MarsTS.Units.Sensors {

    public class DepositSensor : AbstractSensor<IDepositable> {
		
		public override bool IsDetected (IDepositable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}
	}
}