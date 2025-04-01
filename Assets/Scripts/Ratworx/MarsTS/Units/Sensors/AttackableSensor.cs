namespace Ratworx.MarsTS.Units.Sensors {

    public class AttackableSensor : AbstractSensor<IAttackable> {

		public override bool IsDetected (IAttackable unit) {
			return IsDetected(unit.GameObject.transform.root.name);
		}
	}
}