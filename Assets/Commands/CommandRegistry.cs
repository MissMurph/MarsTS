using MarsTS.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

    public class CommandRegistry : NetworkBehaviour {

		private static CommandRegistry instance;

        private Dictionary<string, CommandFactory> registered;

        [SerializeField]
		private CommandFactory[] factoriesToInit;

		private void Awake () {
			instance = this;
			registered = new Dictionary<string, CommandFactory>();
		}

		public override void OnDestroy () {
			base.OnDestroy();
			instance = null;
		}

		public override void OnNetworkSpawn () {
			base.OnNetworkSpawn();

			if (!NetworkManager.Singleton.IsServer) return;
			
			foreach (CommandFactory toConstruct in factoriesToInit) {
				CommandFactory constructed = Instantiate(toConstruct, transform);
				registered[constructed.Name] = constructed;

				NetworkObject commandNetworking = constructed.GetComponent<NetworkObject>();
				commandNetworking.Spawn();

				commandNetworking.transform.parent = transform;

				RegisterCommandClientRpc(commandNetworking);
			}
		}

		[Rpc(SendTo.NotServer)]
		private void RegisterCommandClientRpc (NetworkObjectReference objectRef) {
			if (NetworkManager.Singleton.IsServer) return;

			CommandFactory factoryToRegister = ((GameObject)objectRef).GetComponent<CommandFactory>();

			registered[factoryToRegister.Name] = factoryToRegister;
		}

		public static T Get<T> (string key) where T : CommandFactory  {
			if (instance.registered.TryGetValue(key, out CommandFactory entry)) {
				if (entry is T) return entry as T;
			}

			throw new ArgumentException("Command " + key + " of type " + typeof(T) + " not found");
		}

		public static CommandFactory Get (string key) {
			if (instance.registered.TryGetValue(key, out CommandFactory entry)) {
				return entry;
			}

			throw new ArgumentException("Command " + key + " not found");
		}

		public static bool TryGet<T>(string key, out T command) where T : CommandFactory
		{
			command = default;
			
			if (instance.registered.TryGetValue(key, out CommandFactory entry)) 
			{
				if (entry is T factory)
				{
					command = factory;
					return true;
				}
			}
			
			return false;
		}

		public static bool TryGet(string key, out CommandFactory command)
		{
			command = default;

			if (!instance.registered.TryGetValue(key, out CommandFactory factory)) 
				return false;
			
			command = factory;
			return true;
		}
    }
}