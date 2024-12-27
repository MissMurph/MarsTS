using MarsTS.Events;
using UnityEngine;
using Unity.Netcode;

namespace MarsTS.Research
{
    public class DamageUpgradeTechnology : Technology
    {
        [SerializeField] private int _damageIncrease;

        protected override void Start()
        {
            base.Start();
            
            if (!NetworkManager.Singleton.IsServer) return;
            
            EventBus.AddListener<UnitAttackEvent>(OnUnitAttack);
        }

        private void OnUnitAttack(UnitAttackEvent evnt)
        {
            if (evnt.Unit.Owner != _owner
                || evnt.Phase == Phase.Post
                || evnt.Damage < 0) 
                return;
            
            evnt.SetDamage(evnt.Damage + _damageIncrease);
        }
    }
}