using MarsTS.Events;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Units
{
    public class SniperGun : ProjectileTurret
    {
        //This damage is added to the attack whenever infantry is hit
        [FormerlySerializedAs("bonusDamage")] [SerializeField]
        protected int _bonusDamage;

        protected override void OnHit(bool success, IAttackable unit)
        {
            if (NetworkManager.Singleton.IsServer && success) return;

            int damage = _damage;

            if (unit.GameObject.tag.Equals("Infantry")) damage += _bonusDamage;

            UnitAttackEvent attackEvent = new UnitAttackEvent(_bus, unit as ISelectable, _parent, damage);

            attackEvent.Phase = Phase.Pre;
            _bus.Global(attackEvent);

            // Captures modified damage
            damage = attackEvent.Damage;
            unit.Attack(damage);

            attackEvent.Phase = Phase.Post;
            _bus.Global(attackEvent);
        }
    }
}