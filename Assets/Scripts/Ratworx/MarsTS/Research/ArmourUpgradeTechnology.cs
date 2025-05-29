using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Research
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
            if (evnt.Attackable.GetRelationship(_owner) != Relationship.Owned 
                || evnt.Phase == Phase.Post
                || evnt.Damage < 0) 
                return;
            
            evnt.SetDamage(evnt.Damage - _damageDecrease);
        }
    }
}