using System;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Infantry;
using Ratworx.MarsTS.Units.Squads;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Entities
{
    public class SquadHealthAttribute : MonoBehaviour,
                                        IAttackable
    {
        public GameObject GameObject => gameObject;
        public Entity Entity => _entity;

        public int Health {
            get
            {
                int value = 0;

                foreach (SquadMemberEntry member in _squadManager.MemberEntries) {
                    member.Entity.TryGetEntityComponent(out HealthAttribute health);
                    value += health.Health;
                }

                return value;
            }
        }

        public int MaxHealth {
            get
            {
                int maxHealth = 0;
                
                foreach (SquadMemberEntry member in _squadManager.MemberEntries) {
                    member.Entity.TryGetEntityComponent(out HealthAttribute health);
                    maxHealth = health.MaxHealth;
                    // We want a live one to count bonuses/modifiers, but only need 1 out of the squad
                    break;
                }

                return maxHealth * _squadManager.MaxMembers;
            }
        }

        private SquadManager _squadManager;
        private Entity _entity;
        private EventAgent _eventAgent;
        
        private void Awake() {
            _entity = GetComponent<Entity>();
            _squadManager = GetComponent<SquadManager>();
        }

        private void Start() {
            _squadManager.OnSquadMembershipChanged += OnSquadMembershipChanged;
        }

        private void OnSquadMembershipChanged(SquadMemberEntry member, bool isMember) {
            if (isMember) {
                member.Entity.TryGetEntityComponent(out HealthAttribute health);
                health.OnAttributeChange += OnMemberHealthChanged;
            }
            else {
                member.Entity.TryGetEntityComponent(out HealthAttribute health);
                health.OnAttributeChange -= OnMemberHealthChanged;
            }
        }

        private void OnMemberHealthChanged(int oldValue, int newValue) {
            if (Health <= 0) return;
            
            UnitHurtEvent hurtEvent = new UnitHurtEvent(this, oldValue - newValue);
            hurtEvent.Phase = Phase.Post;
            _eventAgent.PostGlobal(hurtEvent);
        }

        public Relationship GetRelationship(Faction player) => throw new System.NotImplementedException();

        public void Attack(int damage) {
            RatLogger.Warning?.Log($"{typeof(SquadHealthAttribute)} is being attacked! This shouldn't be possible!");
        }
    }
}