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

        private void Awake()
        {
            EventBus.AddListener<TeamsInitEvent>(OnTeamInit);
            EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
        }

        public override void OnNetworkSpawn()
        {
            _playerId = NetworkManager.Singleton.LocalClient.ClientId;
        }

        private void OnTeamInit(TeamsInitEvent @event)
        {
            if (!NetworkManager.Singleton.IsServer
                || @event.Phase != Phase.Post)
                return;

            InstantiateClientControllerClientRpc();
        }

        [Rpc(SendTo.NotServer)]
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