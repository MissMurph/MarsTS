using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Events;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands
{
    public class CommandCache : NetworkBehaviour
    {
        private static CommandCache _instance;

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
        }

        private void Start()
        {
            EventBus.AddListener<CommandCompleteEvent>(OnCommandComplete);
        }

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                // TODO: Refactor this for player count changes IE disconnect
                // Host is considered a connected client, so we check for all -1
                clientCount = NetworkManager.Singleton.ConnectedClients.Count - 1;
            }
        }

        private void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            staleCheckTimer -= Time.deltaTime;

            if (!(staleCheckTimer <= 0f)) return;
            
            CheckStaleToDestroy();
            staleCheckTimer += staleCheckInterval;
        }

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

        public static bool TryGet(int id, out Commandlet output)
        {
            throw new NotImplementedException();
        }

        public static bool TryGet<T>(int id, Commandlet<T> output)
        {
            throw new NotImplementedException();
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

        private void CheckStaleToDestroy()
        {
            foreach (KeyValuePair<int, Commandlet> stale in staleCommands)
            {
                if (staleCheckRequests.ContainsKey(stale.Key))
                {
                    Debug.Log($"{stale.Key} in ongoing requests, skipping");
                    continue;
                }
                
                Debug.Log($"Checking clients if {stale.Key} is stale");
                staleCheckRequests[stale.Key] = new List<bool>();
                CheckStaleClientRpc(stale.Key);
            }
        }

        [Rpc(SendTo.NotServer)]
        private void CheckStaleClientRpc(int id)
        {
            //Debug.Log($"checking if {id} is stale on this client");
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
                //Debug.Log($"{id} is stale, destroying");
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