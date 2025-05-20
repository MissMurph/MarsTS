using System;
using Ratworx.MarsTS.Entities;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry
{
    public class InfantryMembership : NetworkBehaviour
    {
        /// <remarks><c>InfantrySquad</c> will be null of being removed from a squad.</remarks>
        public Action<InfantrySquad> OnSquadMembershipChanged;
        
        [SerializeField] private SquadManager _squad;
        [SerializeField] private MonoBehaviour[] _squadDisablingComponents;

        public void SetSquad(SquadManager squad) {
            _squad = squad;

            if (squad is not null) {
                foreach (MonoBehaviour component in _squadDisablingComponents) {
                    component.enabled = false;
                }
            }
            
            SetSquadClientRpc(squad.name);
        }

        [Rpc(SendTo.NotServer)]
        private void SetSquadClientRpc(string squadEntityName) {
            if (!EntityCache.TryGetEntityComponent(squadEntityName, out SquadManager squad)) {
                Debug.LogError($"Couldn't find {typeof(SquadManager)} with name {squadEntityName}!");
                return;
            }

            _squad = squad;
        }

        public void ClearSquad() {
            _squad = null;
            
            foreach (MonoBehaviour component in _squadDisablingComponents) {
                component.enabled = true;
            }
            
            ClearSquadClientRpc();
        }

        [Rpc(SendTo.NotServer)]
        private void ClearSquadClientRpc() {
            _squad = null;
        }
    }
}