namespace Ratworx.MarsTS.Units.Sensors {

    public class SelectableSensor : AbstractSensor<ISelectable> {

		public override bool IsDetected (ISelectable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}
	}
}