using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Events;
using MarsTS.Prefabs;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands
{
    public class CommandletsCache : NetworkBehaviour
    {
        private static CommandletsCache _instance;

        [SerializeField]
        private float staleCheckInterval = 10f;
        private float staleCheckTimer;

        private int clientCount = 0;

        private static int _instanceCount;

        private Dictionary<int, Commandlet> activeCommands;
        private Dictionary<int, Commandlet> staleCommands;
        private Dictionary<int, List<bool>> staleCheckRequests;

        public static int Count => _instance.activeCommands.Count;

        public Commandlet this[int id] => _instance.activeCommands[id];

        private void Awake()
        {
            _instance = this;

            _instanceCount = 1;
            staleCheckTimer = staleCheckInterval;
            
            activeCommands = new Dictionary<int, Commandlet>();
            staleCommands = new Dictionary<int, Commandlet>();
            staleCheckRequests = new Dictionary<int, List<bool>>();

            if (!NetworkManager.Singleton.IsServer) return;

            GameInit.OnSpawnSystems += OnSystemsSpawn;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void Start()
        {
            EventBus.AddListener<CommandCompleteEvent>(OnCommandComplete);
        }

        private void OnSystemsSpawn()
        {
            // Host is considered a connected client, so we check for all -1
            clientCount = NetworkManager.Singleton.ConnectedClients.Count - 1;
        }

        private void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            staleCheckTimer -= Time.deltaTime;

            if (!(staleCheckTimer <= 0f)) return;
            
            CheckStaleToDestroy();
            staleCheckTimer += staleCheckInterval;
        }

        private void OnClientConnected(ulong clientId) => clientCount++;

        private void OnClientDisconnected(ulong clientId) => clientCount--;

        public static int Register(Commandlet commandlet)
        {
            if (commandlet.Id > 0)
            {
                if (!NetworkManager.Singleton.IsServer)
                    return _instance.activeCommands.TryAdd(commandlet.Id, commandlet) ? _instanceCount : -1;

                Debug.LogError($"Attempting to register already registered command {commandlet.Name}:{commandlet.Id}!");
                return -1;
            }
            
            _instanceCount++;
            return _instance.activeCommands.TryAdd(_instanceCount, commandlet) ? _instanceCount : -1;
        }

        private static void OnCommandComplete(CommandCompleteEvent _event)
        {
            int id = _event.Command.Id;
            
            if (!_instance.activeCommands.ContainsKey(id) || _event.Command.commandedUnits.Count > 0) 
                return;
            
            if (_instance.staleCommands.TryAdd(id, _event.Command))
                _instance.activeCommands.Remove(id);
            else 
                Debug.LogError($"Error marking command {_event.Command.Name}:{_event.Command.Id} as stale");
        }

        private void CheckStaleToDestroy() {
            List<int> checkRequestsToSend = new();

            foreach (KeyValuePair<int, Commandlet> stale in staleCommands)
            {
                if (staleCheckRequests.ContainsKey(stale.Key))
                {
                    continue;
                }
                
                staleCheckRequests[stale.Key] = new List<bool>();
                checkRequestsToSend.Add(stale.Key);
            }
            
            foreach (int id in checkRequestsToSend)
            {
                CheckStaleClientRpc(id);    
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void CheckStaleClientRpc(int id)
        {
            bool result;

            if (!activeCommands.ContainsKey(id) && !staleCommands.ContainsKey(id))
                result = true;
            else
                result = IsStale(id);
            
            SendStaleResponsesServerRpc(id, result);
        }

        [Rpc(SendTo.Server)]
        private void SendStaleResponsesServerRpc(int id, bool isStale)
        {
            if (!staleCheckRequests.ContainsKey(id)) return;

            staleCheckRequests[id].Add(isStale);

            if (staleCheckRequests[id].Count >= clientCount)
            {
                Destroy(staleCommands[id].gameObject);
                
                staleCheckRequests.Remove(id);
                staleCommands.Remove(id);
                
                DeleteCacheEntryClientRpc(id);
            }
        }

        [Rpc(SendTo.NotServer)]
        private void DeleteCacheEntryClientRpc(int id)
        {
            activeCommands.Remove(id);
            staleCommands.Remove(id);
        }

        public static bool IsStale(int id) => _instance.staleCommands.ContainsKey(id);

        public override void OnDestroy()
        {
            _instance = null;
            base.OnDestroy();
        }
    }
}