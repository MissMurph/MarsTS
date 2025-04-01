using MarsTS.Events;
using UnityEngine;
using Unity.Netcode;

namespace MarsTS.Research
{
    public class ArmourUpgradeTechnology : Technology
    {
        [SerializeField] private int _damageDecrease;

        protected override void Start()
        {
            base.Start();

            if (!NetworkManager.Singleton.IsServer) return;
            
            EventBus.AddListener<UnitHurtEvent>(OnUnitHurt);
        }

        private void OnUnitHurt(UnitHurtEvent evnt)
        {
            if (evnt.Unit.Owner != _owner 
                || evnt.Phase == Phase.Post
                || evnt.Damage < 0) 
                return;
            
            evnt.SetDamage(evnt.Damage - _damageDecrease);
        }
    }
}