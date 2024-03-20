using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
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

		private Dictionary<ulong, Faction> players = new Dictionary<ulong, Faction>();

		private EventAgent bus;

		private void Awake () {
			bus = GetComponent<EventAgent>();
		}

		private void Start () {
			NetworkManager.Singleton.OnServerStarted += OnServerStart;
			NetworkManager.Singleton.OnClientStarted += OnClientStart;
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
			players[id] = NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<Faction>();
		}

		private void OnGameStart () {
			TransmitGameStartServerRpc();
		}

		[ServerRpc]
		private void TransmitGameStartServerRpc () {
			TeamCache.SetTeams();
			
			int count = 0;

			foreach (Faction player in players.Values) {
				NetworkObject hqNetwork = Instantiate(headquartersPrefab, startPositions[count].position, startPositions[count].rotation);
				ISelectable hq = hqNetwork.GetComponent<ISelectable>();
				hqNetwork.Spawn();
				hq.SetOwner(player);

				count++;
			}

			TransmitStartVisionClientRpc();
		}

		[ClientRpc]
		private void TransmitStartVisionClientRpc () {
			bus.Global(new PlayerInitEvent(bus));
		}
	}
}