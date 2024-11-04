using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Events;
using MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Players {

    public class NetworkPlayer : NetworkBehaviour {

        [SerializeField]
        private Player clientPrefab;

        private ulong _playerId;

        private Faction _faction;
        
        private void Awake() {
            EventBus.AddListener<TeamsInitEvent>(OnTeamInit);
        }

        public override void OnNetworkSpawn () {
            _playerId = NetworkManager.Singleton.LocalClient.ClientId;
        }

        private void OnTeamInit(TeamsInitEvent @event) {
            if (!NetworkManager.Singleton.IsServer
                || @event.Phase != Phase.Post) 
                return;
            
            InstantiateClientControllerClientRpc();
        }

        [Rpc(SendTo.Owner)]
        private void InstantiateClientControllerClientRpc() {
            _faction = TeamCache.GetAssignedFaction(_playerId);
            
            Instantiate(clientPrefab, null)
                .GetComponent<Player>()
                .SetCommander(_faction);
        }
    }
}