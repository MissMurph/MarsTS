using System;
using System.Collections.Generic;
using MarsTS.Prefabs;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands
{
    public class CommandPrimer : NetworkBehaviour
    {
        private static CommandPrimer _instance;

        private Dictionary<string, CommandFactory> _registered;
        
        private void Awake()
        {
            _instance = this;
            _registered = new Dictionary<string, CommandFactory>();
            
            if (!NetworkManager.Singleton.IsServer) return;

            GameInit.OnSpawnSystems += SpawnCommandFactories;
        }

        public override void OnDestroy()
        {
            _instance = null;
            base.OnDestroy();
        }
        
        private void SpawnCommandFactories()
        {
            if (!Registry.TryGetPrefabRegistry("command_factories", out IPrefabRegistry registry))
            {
                Debug.LogError($"Couldn't find Command Factories registry!");
                return;
            }
            
            foreach ((string key, GameObject prefab) in registry.GetAllPrefabs())
            {
                SpawnFactory(key, prefab);
            }
        }

        private void SpawnFactory(string key, GameObject prefab)
        {
            GameObject instantiated = Instantiate(prefab);
            CommandFactory factory = instantiated.GetComponent<CommandFactory>();
            NetworkObject networkObject = instantiated.GetComponent<NetworkObject>();

            networkObject.Spawn();

            networkObject.TrySetParent(transform);
            
            RegisterFactory(key, factory);
            RegisterCommandClientRpc(key, networkObject);
        }

        private void RegisterFactory(string key, CommandFactory factory) => _registered[key] = factory;

        [Rpc(SendTo.NotServer)]
        private void RegisterCommandClientRpc(string key, NetworkObjectReference netRef)
        {
            if (!netRef.TryGet(out NetworkObject networkObject)
                || !networkObject.TryGetComponent(out CommandFactory factory))
            {
                Debug.LogError($"Couldn't find registered prefab {key}!");
                return;
            }
            
            RegisterFactory(key, factory);
        }
        
        public static T Get<T>(string key) where T : CommandFactory
        {
            if (!_instance._registered.TryGetValue(key, out CommandFactory entry))
                throw new ArgumentException($"Command {key} of type {typeof(T)} not found!");

            if (entry is T factory)
                return factory;
                
            throw new ArgumentException($"Command {key} is not of type {typeof(T)}, it's {entry.GetType()}");
        }

        public static CommandFactory Get(string key)
        {
            if (_instance._registered.TryGetValue(key, out CommandFactory entry)) 
                return entry;

            throw new ArgumentException($"Command {key} not found!");
        }

        public static bool TryGet<T>(string key, out T command) where T : CommandFactory
        {
            if (_instance._registered.TryGetValue(key, out CommandFactory entry))
            {
                if (entry is T factory)
                {
                    command = factory;
                    return true;
                }

                Debug.LogWarning($"Command {key} doesn't match expected type {typeof(T)}, it's {entry.GetType()}");
            }
            
            command = default;
            return false;
        }

        public static bool TryGet(string key, out CommandFactory command)
        {
            if (!_instance._registered.TryGetValue(key, out CommandFactory factory))
            {
                command = default;
                return false;
            }

            command = factory;
            return true;
        }
    }
}