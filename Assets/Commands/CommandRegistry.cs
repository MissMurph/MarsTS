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

		public override void OnNetworkSpawn () {
			base.OnNetworkSpawn();

			foreach (CommandFactory toConstruct in factoriesToInit) {
				CommandFactory constructed = Instantiate(toConstruct);
				registered[constructed.Name] = constructed;

				NetworkObject commandNetworking = constructed.GetComponent<NetworkObject>();
				commandNetworking.Spawn();
				commandNetworking.TrySetParent(transform);

				RegisterCommandClientRpc(commandNetworking);
			}
		}

		private void Start () {
			EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
		}

		private void OnPlayerInit (PlayerInitEvent _event) {
			if (_event.Phase.Equals(Phase.Post)) return;

			foreach (CommandFactory toConstruct in factoriesToInit) {
				CommandFactory constructed = Instantiate(toConstruct, transform);
				registered[constructed.Name] = constructed;

				NetworkObject commandNetworking = constructed.GetComponent<NetworkObject>();
				//commandNetworking.Spawn();

				//RegisterCommandClientRpc(commandNetworking);
			}
		}

		[ClientRpc]
		private void RegisterCommandClientRpc (NetworkObjectReference objectRef) {
			if (NetworkManager.Singleton.IsServer) return;

			CommandFactory factoryToRegister = ((GameObject)objectRef).GetComponent<CommandFactory>();

			Debug.Log(factoryToRegister.Name);

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

		private void OnDestroy () {
			instance = null;
		}
	}
}