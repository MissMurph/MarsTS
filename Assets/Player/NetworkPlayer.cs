using System.Collections.Generic;
using MarsTS.Events;
using MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Players
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [FormerlySerializedAs("clientPrefab")] [SerializeField]
        private Player _clientPrefab;

        [SerializeField] private ulong _playerId;

        private Faction _faction;

        public override void OnNetworkSpawn()
        {
            // This is being set on both the client objects
            _playerId = NetworkObject.OwnerClientId;

            if (NetworkManager.LocalClient.ClientId != _playerId) 
                return;
            
            EventBus.AddListener<TeamsInitEvent>(OnTeamInit);
            EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
        }

        private void OnTeamInit(TeamsInitEvent @event)
        {
            if (!NetworkManager.Singleton.IsServer
                || @event.Phase != Phase.Post)
                return;

            InstantiateClientControllerClientRpc();
        }

        [Rpc(SendTo.Owner)]
        private void InstantiateClientControllerClientRpc()
        {
            _faction = TeamCache.GetAssignedFaction(_playerId);

            Instantiate(_clientPrefab, null)
                .GetComponent<Player>()
                .SetCommander(_faction);
        }

        private void OnPlayerInit(PlayerInitEvent _event)
        {
            TransmitPlayerReadyServerRpc(_playerId);
        }

        [Rpc(SendTo.Server)]
        private void TransmitPlayerReadyServerRpc(ulong id)
        {
            GameInit.PlayerReady(id);
        }
    }
}