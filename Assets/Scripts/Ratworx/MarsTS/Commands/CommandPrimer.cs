using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.UI;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Registry;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands
{
    public class CommandPrimer : NetworkBehaviour
    {
        private static CommandPrimer _instance;

        private Dictionary<string, CommandFactory> _registeredFactories;
        private Dictionary<string, ICommandInterface> _registeredInterfaces;
        
        private void Awake()
        {
            _instance = this;
            _registeredFactories = new Dictionary<string, CommandFactory>();
            _registeredInterfaces = new Dictionary<string, ICommandInterface>();
            
            if (!NetworkManager.Singleton.IsServer) return;

            GameInit.OnSpawnSystems += SpawnCommandFactories;
            GameInit.OnSpawnSystems += SpawnCommandInterfaces;
        }

        public override void OnDestroy()
        {
            _instance = null;
            base.OnDestroy();
        }

        private void SpawnCommandInterfaces() {
            if (!Registry.Registry.TryGetPrefabRegistry("command_interfaces", out IPrefabRegistry registry))
            {
                Debug.LogError($"Couldn't find Command Interfaces registry!");
                return;
            }
            
            foreach ((string key, GameObject prefab) in registry.GetAllPrefabs())
            {
                SpawnInterface(key, prefab);
            }
        }

        private void SpawnInterface(string key, GameObject prefab)
        {
            GameObject instantiated = Instantiate(prefab);
            ICommandInterface commandInterface = instantiated.GetComponent<ICommandInterface>();

            if (instantiated.TryGetComponent<NetworkObject>(out NetworkObject networkObject)) {
                networkObject.Spawn();
                networkObject.TrySetParent(transform);
                RegisterCommandInterfaceClientRpc(key, networkObject);
            }
            else
                instantiated.transform.parent = transform;
            
            RegisterInterface(key, commandInterface);
        }

        private void RegisterInterface(string key, ICommandInterface commandInterface)
            => _registeredInterfaces[key] = commandInterface;

        [Rpc(SendTo.NotServer)]
        private void RegisterCommandInterfaceClientRpc(string key, NetworkObjectReference netRef)
        {
            if (!netRef.TryGet(out NetworkObject networkObject)
                || !networkObject.TryGetComponent(out ICommandInterface commandInterface))
            {
                Debug.LogError($"Couldn't find registered prefab {key}!");
                return;
            }
            
            RegisterInterface(key, commandInterface);
        }

        private void SpawnCommandFactories()
        {
            if (!Registry.Registry.TryGetPrefabRegistry("command_factories", out IPrefabRegistry registry))
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

        private void RegisterFactory(string key, CommandFactory factory) => _registeredFactories[key] = factory;

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

        /// <remarks>Use this for generic types.</remarks>
        public static T GetFactory<T>() where T : CommandFactory {
            foreach (CommandFactory factory in _instance._registeredFactories.Values) {
                if (factory is T commandFactory) 
                    return commandFactory;
            }
            
            RatLogger.Error?.Log($"No Command Factory of type {nameof(T)} registered! Returning null!");
            return null;
        }
        
        public static T GetFactory<T>(string key) where T : CommandFactory
        {
            if (!_instance._registeredFactories.TryGetValue(key, out CommandFactory entry))
                throw new ArgumentException($"Command {key} of type {typeof(T)} not found!");

            if (entry is T factory)
                return factory;
                
            throw new ArgumentException($"Command {key} is not of type {typeof(T)}, it's {entry.GetType()}");
        }

        public static CommandFactory GetFactory(string key)
        {
            if (_instance._registeredFactories.TryGetValue(key, out CommandFactory entry)) 
                return entry;

            throw new ArgumentException($"Command {key} not found!");
        }

        public static bool TryGetFactory<T>(string key, out T command) where T : CommandFactory
        {
            if (_instance._registeredFactories.TryGetValue(key, out CommandFactory entry))
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

        public static bool TryGetFactory(string key, out CommandFactory command)
        {
            if (!_instance._registeredFactories.TryGetValue(key, out CommandFactory factory))
            {
                command = default;
                return false;
            }

            command = factory;
            return true;
        }

        public static ICommandInterface GetInterface(string key) {
            if (_instance._registeredInterfaces.TryGetValue(key, out ICommandInterface entry)) 
                return entry;

            throw new ArgumentException($"Command Interface {key} not found!");
        }

        public static bool TryGetInterface(string key, out ICommandInterface commandInterface)
            => _instance._registeredInterfaces.TryGetValue(key, out commandInterface);

        public static T GetInterface<T>(string key) where T : ICommandInterface {
            if (!_instance._registeredInterfaces.TryGetValue(key, out ICommandInterface entry)) {
                RatLogger.Error?.Log($"Command Interface {key} not found!");
                return default;
            }

            if (entry is T output) 
                return output;
            
            RatLogger.Error?.Log(
                $"Command Interface {key} not expected type {nameof(T)}, is {entry.GetType()} instead!");
            return default;
        }
    }
}