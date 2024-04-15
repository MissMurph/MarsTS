using MarsTS.Events;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Players {

    public class PlayerResource : NetworkBehaviour {

        public string Key { get { return key; } }

        [SerializeField]
        private string key;

        public int Amount {
            get {
                return storedResources.Value;
            }
            private set {
                storedResources.Value = value;
            }
        }

        private NetworkVariable<int> storedResources = new(writePerm:NetworkVariableWritePermission.Server);

        [SerializeField]
        private int startingAmount;

        private EventAgent bus;

        private Faction player;

		private void Awake () {
            bus = GetComponent<EventAgent>();
            player = GetComponent<Faction>();
		}

		public override void OnNetworkSpawn () {
			base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsClient) {
                AttachClientListeners();
            }
		}

		private void Start () {
            if (NetworkManager.Singleton.IsServer) {
                Amount = startingAmount;
            }
		}

		private void AttachClientListeners () {
            storedResources.OnValueChanged += OnResourceValueChange;
        }

        private void OnResourceValueChange (int _oldAmount, int _newAmount) {
			bus.Global(new ResourceUpdateEvent(bus, player, this));
		}

		public bool Deposit (int amount) {
            Amount += amount;
            return true;
        }

        public bool Withdraw (int amount) {
            if (Amount >= amount) {
                Amount -= amount;
				return true;
            }
            else return false;
        }
	}
}