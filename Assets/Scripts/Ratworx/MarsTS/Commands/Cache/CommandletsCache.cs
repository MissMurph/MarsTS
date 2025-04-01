using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands.Cache
{
    public class CommandletsCache : NetworkBehaviour
    {
        private static CommandletsCache _instance;

        [FormerlySerializedAs("staleCheckInterval")]
        [SerializeField]
        private float _staleCheckInterval = 10f;
        private float _staleCheckTimer;

        private int _clientCount = 0;

        private static int _instanceCount;

        private Dictionary<int, Commandlet> _activeCommands;
        private Dictionary<int, Commandlet> _staleCommands;
        private Dictionary<int, List<bool>> _staleCheckRequests;

        public static int Count => _instance._activeCommands.Count;

        public Commandlet this[int id] => _instance._activeCommands[id];

        private void Awake()
        {
            _instance = this;

            _instanceCount = 1;
            _staleCheckTimer = _staleCheckInterval;
            
            _activeCommands = new Dictionary<int, Commandlet>();
            _staleCommands = new Dictionary<int, Commandlet>();
            _staleCheckRequests = new Dictionary<int, List<bool>>();

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
            _clientCount = NetworkManager.Singleton.ConnectedClients.Count - 1;
        }

        private void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            _staleCheckTimer -= Time.deltaTime;

            if (!(_staleCheckTimer <= 0f)) return;

            CheckCommandletsForStale();
            CheckStaleToDestroy();
            _staleCheckTimer += _staleCheckInterval;
        }

        private void OnClientConnected(ulong clientId) => _clientCount++;

        private void OnClientDisconnected(ulong clientId) => _clientCount--;

        public static int Register(Commandlet commandlet)
        {
            if (commandlet.Id > 0)
            {
                if (!NetworkManager.Singleton.IsServer)
                    return _instance._activeCommands.TryAdd(commandlet.Id, commandlet) ? _instanceCount : -1;

                Debug.LogError($"Attempting to register already registered command {commandlet.Name}:{commandlet.Id}!");
                return -1;
            }
            
            _instanceCount++;
            return _instance._activeCommands.TryAdd(_instanceCount, commandlet) ? _instanceCount : -1;
        }

        public static bool TryGet(int id, out Commandlet order) => _instance._activeCommands.TryGetValue(id, out order);

        private static void OnCommandComplete(CommandCompleteEvent _event)
        {
            int id = _event.Command.Id;
            
            if (!_instance._activeCommands.ContainsKey(id) || _event.Command.commandedUnits.Count > 0) 
                return;
            
            if (_instance._staleCommands.TryAdd(id, _event.Command))
                _instance._activeCommands.Remove(id);
            else 
                Debug.LogError($"Error marking command {_event.Command.Name}:{_event.Command.Id} as stale");
        }

        private void CheckCommandletsForStale() {
            var activeToMarkStale = new List<int>();
            
            foreach (Commandlet activeCommand in _activeCommands.Values) {
                if (_staleCommands.ContainsKey(activeCommand.Id)) continue;

                if (activeCommand.commandedUnits.Count > 0) continue;
                
                if (!_staleCommands.TryAdd(activeCommand.Id, activeCommand));
                    Debug.LogError($"Error marking command {activeCommand.Name}:{activeCommand.Id} as stale");
            }
            
            foreach (Commandlet staleCommand in _staleCommands.Values) {
                if (!_activeCommands.ContainsKey(staleCommand.Id)) continue;

                _activeCommands.Remove(staleCommand.Id);
            }
        }

        private void CheckStaleToDestroy() {
            var checkRequestsToSend = new List<int>();

            foreach (KeyValuePair<int, Commandlet> stale in _staleCommands)
            {
                if (_staleCheckRequests.ContainsKey(stale.Key))
                {
                    continue;
                }
                
                _staleCheckRequests[stale.Key] = new List<bool>();
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

            if (!_activeCommands.ContainsKey(id) && !_staleCommands.ContainsKey(id))
                result = true;
            else
                result = IsStale(id);
            
            SendStaleResponsesServerRpc(id, result);
        }

        [Rpc(SendTo.Server)]
        private void SendStaleResponsesServerRpc(int id, bool isStale)
        {
            if (!_staleCheckRequests.ContainsKey(id)) return;

            _staleCheckRequests[id].Add(isStale);

            if (_staleCheckRequests[id].Count >= _clientCount)
            {
                Destroy(_staleCommands[id].gameObject);
                
                _staleCheckRequests.Remove(id);
                _staleCommands.Remove(id);
                
                DeleteCacheEntryClientRpc(id);
            }
        }

        [Rpc(SendTo.NotServer)]
        private void DeleteCacheEntryClientRpc(int id)
        {
            _activeCommands.Remove(id);
            _staleCommands.Remove(id);
        }

        public static bool IsStale(int id) => _instance._staleCommands.ContainsKey(id);

        public override void OnDestroy()
        {
            _instance = null;
            base.OnDestroy();
        }
    }
}