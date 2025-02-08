using System;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Editor;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.UI;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS
{
    public class GameInit : MonoBehaviour
    {
        public static event Action OnSpawnSystems
        {
            add
            {
                if (Instance._hasSystemSpawnEventFired)
                    value.Invoke();
                else
                    Instance._onSpawnSystems += value;
            }
            remove => Instance._onSpawnSystems -= value;
        }
        
        private event Action _onSpawnSystems;
        private bool _hasSystemSpawnEventFired;
        
        public static event Action OnSpawnPlayers;
        public static event Action OnSpawnEntities
        {
            add
            {
                if (Instance._hasSpawnEventFired)
                    value.Invoke();
                else
                    Instance._onSpawnEntities += value;
            }
            remove => Instance._onSpawnEntities -= value;
        }

        private event Action _onSpawnEntities;
        private bool _hasSpawnEventFired;
        
        private static GameInit Instance;
        
        [SerializeField] private GameStartButton gameStartButton;

        [SerializeField] private Transform canvas;

        [SerializeField] private EntitySpawner[] startPositions;

        [SerializeField] private NetworkObject headquartersPrefab;

        [SerializeField] private NetworkObject teamCachePrefab;

        [SerializeField] private NetworkObject commandRegistryPrefab;

        [SerializeField] private NetworkObject commandCachePrefab;

        private readonly HashSet<ulong> _players = new HashSet<ulong>();
        private readonly Dictionary<ulong, bool> _playersReadyToStart = new Dictionary<ulong, bool>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // NetworkManager.Singleton.OnServerStarted += OnServerStart;
            // NetworkManager.Singleton.OnClientStarted += OnClientStart;
            
            if (NetworkManager.Singleton.IsServer) OnServerStart();
            if (NetworkManager.Singleton.IsClient) OnClientStart();

            EventBus.AddListener<PlayerInitEvent>(OnClientPlayerInit);

            foreach (EntitySpawner spawner in startPositions)
            {
                spawner.SetDeferredSpawn(true);
            }
            
            
        }

        private void OnClientStart()
        {
            GameStartButton button = Instantiate(gameStartButton, canvas);

            //if (NetworkManager.Singleton.Is) ;

            button.StartGame += OnGameStart;

            button.Init();
        }

        private void OnServerStart()
        {
            ulong serverId = NetworkManager.Singleton.LocalClientId;

            Instance._players.Add(serverId); 
            Instance._playersReadyToStart[serverId] = true;
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientPlayerInit(PlayerInitEvent _event)
        {
           // SendPlayerReadyServerRpc(NetworkManager.Singleton.LocalClient.ClientId);
        }

        public static void PlayerReady(ulong id)
        {
            if (!Instance._playersReadyToStart.TryGetValue(id, out bool value))
                Debug.LogWarning($"Client {id} not cached!");
            else
                Instance._playersReadyToStart[id] = true;
            
            // TODO: Verbose logging
            //Debug.Log($"{id} ready");

            if (Instance._playersReadyToStart.Values.Any(ready => !ready))
            {
                return;
            }

            Instance.SpawnHeadquarters();
        }

        private void OnClientConnected(ulong id)
        {
            _players.Add(id);
            _playersReadyToStart[id] = false;
        }

        private void OnClientDisconnected(ulong id)
        {
            _players.Remove(id);
            _playersReadyToStart.Remove(id);
        }

        private void OnGameStart()
        {
            TransmitGameStartServerRpc();
        }

        [Rpc(SendTo.Server)]
        private void TransmitGameStartServerRpc() => InitializeGameManagers();

        private void InitializeGameManagers()
        {
            Instantiate(teamCachePrefab, transform).Spawn();
            TeamCache.Init(_players.ToArray());
            //Instantiate(commandRegistryPrefab, transform).Spawn();
            Instantiate(commandCachePrefab, transform).Spawn();
        }

        private void SpawnHeadquarters()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            _hasSpawnEventFired = true;
            _onSpawnEntities?.Invoke();

            List<Faction> players = TeamCache.Players;

            int spawnedCount = 0;

            foreach (Faction toSpawnHqFor in players)
            {
                if (toSpawnHqFor.Id == 0) continue;

                EntitySpawner spawner = startPositions[spawnedCount];

                spawner.SetOwner(toSpawnHqFor.Id);

                spawner.SpawnEntity();

                spawnedCount++;
            }
        }
    }
}