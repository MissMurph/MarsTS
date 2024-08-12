using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS {

    public class GameInit : MonoBehaviour {

        [SerializeField]
        private GameStartButton gameStartButton;

		[SerializeField]
		private Transform canvas;

		[SerializeField]
		private Transform[] startPositions;

		[SerializeField]
		private NetworkObject headquartersPrefab;

		[SerializeField]
		private NetworkObject teamCachePrefab;

		[SerializeField]
		private NetworkObject commandRegistryPrefab;

		[SerializeField] private NetworkObject commandCachePrefab;

		private Dictionary<ulong, GameObject> players = new Dictionary<ulong, GameObject>();

		private void Start () {
			NetworkManager.Singleton.OnServerStarted += OnServerStart;
			NetworkManager.Singleton.OnClientStarted += OnClientStart;

			EventBus.AddListener<TeamsInitEvent>(SpawnHeadquarters);
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
			players[id] = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject;
		}

		private void OnGameStart () {
			TransmitGameStartServerRpc();
		}

		[Rpc(SendTo.Server)]
		private void TransmitGameStartServerRpc () {
			//TeamCache.SetTeams();
			
			//int count = 0;

			

			Instantiate(teamCachePrefab, transform).Spawn();

			TeamCache.Init(players.Keys.ToArray());
		}

		private void SpawnHeadquarters (TeamsInitEvent _event) {
			if (_event.Phase.Equals(Phase.Pre)) {
				Instantiate(commandRegistryPrefab, transform).Spawn();
				Instantiate(commandCachePrefab, transform).Spawn();
			}
			else {
				List<Faction> players = TeamCache.Players;

				int spawnedCount = 0;

				foreach (Faction toSpawnHqFor in players) {
					if (toSpawnHqFor.Id == 0) continue;

					NetworkObject hqNetwork = Instantiate(headquartersPrefab, startPositions[spawnedCount].position, startPositions[spawnedCount].rotation);
					ISelectable hq = hqNetwork.GetComponent<ISelectable>();
					hqNetwork.Spawn();
					hq.SetOwner(toSpawnHqFor);

					spawnedCount++;
				}
			}
		}
	}
}