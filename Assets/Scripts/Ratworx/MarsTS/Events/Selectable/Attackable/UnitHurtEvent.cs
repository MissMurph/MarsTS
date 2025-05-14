using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable.Attackable {

	public class UnitHurtEvent : UnitEvent {

		public IAttackable Attackable { get; private set; }
		public int Damage { get; private set; }

		public UnitHurtEvent (IAttackable unit, int damage) : base("Hurt", unit.Entity) {
			Attackable = unit;
			Damage = damage;
		}

		public void SetDamage(int newDamage)
		{
			Damage = newDamage;
		}
	}
}