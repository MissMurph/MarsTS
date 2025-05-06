using System;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class SniperDamageBonus : MonoBehaviour
    {
        //This damage is added to the attack whenever infantry is hit
        [SerializeField] protected int _bonusDamage;

        private EventAgent _eventAgent;

        private void Awake() {
            _eventAgent = GetComponentInParent<EventAgent>();
        }

        private void Start() {
            _eventAgent.AddListener<UnitAttackEvent>(OnUnitAttack);
        }

        private void OnUnitAttack(UnitAttackEvent evnt) {
            if (evnt.Phase == Phase.Post) return;
            
            evnt.SetDamage(evnt.Damage + _bonusDamage);
        }
    }
}