using MarsTS.Units;

namespace MarsTS.Events
{
    public class UnitAttackEvent : SelectableEvent
    {
        public ISelectable Attacker { get; private set; }
        public int Damage { get; private set; }

        public UnitAttackEvent(
            EventAgent source, 
            ISelectable victim, 
            ISelectable attacker, 
            int damage
        ) : base(
            "Attack", 
            source, 
            victim
        )
        {
            Attacker = attacker;
            Damage = damage;
        }

        public void SetDamage(int newDamage)
        {
            Damage = newDamage;
        }
    }
}