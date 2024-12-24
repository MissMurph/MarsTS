using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Editor;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS {

    public class GameInit : MonoBehaviour {

        [SerializeField]
        private GameStartButton gameStartButton;

		[SerializeField]
		private Transform canvas;

		[SerializeField]
		private EntitySpawner[] startPositions;

		[SerializeField]
		private NetworkObject headquartersPrefab;

		[SerializeField]
		private NetworkObject teamCachePrefab;

		[SerializeField]
		private NetworkObject commandRegistryPrefab;

		[SerializeField] private NetworkObject commandCachePrefab;

		private readonly Dictionary<ulong, GameObject> _players = new Dictionary<ulong, GameObject>();

		private void Start () {
			NetworkManager.Singleton.OnServerStarted += OnServerStart;
			NetworkManager.Singleton.OnClientStarted += OnClientStart;

			EventBus.AddListener<PlayerInitEvent>(SpawnHeadquarters);
			
			foreach (var spawner in startPositions)
			{
				spawner.SetDeferredSpawn(true);
			}
		}

		private void OnClientStart () {
			GameStartButton button = Instantiate(gameStartButton, canvas);

			//if (NetworkManager.Singleton.Is) ;

			button.StartGame += OnGameStart;

			button.Init();
		}

		private void OnServerStart () {
			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
		}

		private void OnClientConnected (ulong id) {
			_players[id] = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject;
		}

		private void OnGameStart () {
			TransmitGameStartServerRpc();
		}

		[Rpc(SendTo.Server)]
		private void TransmitGameStartServerRpc() => InitializeGameManagers();

		private void InitializeGameManagers() {
			Instantiate(teamCachePrefab, transform).Spawn();
			TeamCache.Init(_players.Keys.ToArray());
			Instantiate(commandRegistryPrefab, transform).Spawn();
			Instantiate(commandCachePrefab, transform).Spawn();
		}

		private void SpawnHeadquarters(PlayerInitEvent @event) {
			if (!NetworkManager.Singleton.IsServer) return;
			
			List<Faction> players = TeamCache.Players;

			int spawnedCount = 0;

			foreach (Faction toSpawnHqFor in players) {
				if (toSpawnHqFor.Id == 0) continue;

				var spawner = startPositions[spawnedCount];
				
				spawner.SetOwner(toSpawnHqFor.Id);
				
				spawner.SpawnEntity();

				spawnedCount++;
			}
		}
    }
}