using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Research
{
    public class DamageUpgradeTechnology : Technology
    {
        [SerializeField] private int _damageIncrease;

        protected override void Start() {
            base.Start();

            if (!NetworkManager.Singleton.IsServer) return;

            EventBus.AddListener<UnitAttackEvent>(OnUnitAttack);
        }

        private void OnUnitAttack(UnitAttackEvent evnt) {
            if (evnt.VictimAttackable.GetRelationship(_owner) != Relationship.Owned
                || evnt.Phase == Phase.Post
                || evnt.Damage < 0)
                return;

            evnt.SetDamage(evnt.Damage + _damageIncrease);
        }
    }
}