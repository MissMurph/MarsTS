using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Teams;
using Scenes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Player
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

            EventBus.AddListener<TeamsInitEvent>(OnTeamInit);
            
            if (NetworkManager.LocalClient.ClientId != _playerId) 
                return;

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