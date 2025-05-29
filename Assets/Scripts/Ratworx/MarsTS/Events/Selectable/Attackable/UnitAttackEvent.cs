using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable.Attackable
{
    public class UnitAttackEvent : UnitEvent
    {
        public IAttackable VictimAttackable { get; private set; }
        public Entity Attacker { get; private set; }
        public int Damage { get; private set; }

        public UnitAttackEvent(
            IAttackable victim, 
            Entity attacker, 
            int damage
        ) : base(
            "Attack",
            victim.Entity
        ) {
            Attacker = attacker;
            Damage = damage;
            VictimAttackable = victim;
        }

        public void SetDamage(int newDamage) {
            Damage = newDamage;
        }
    }
}