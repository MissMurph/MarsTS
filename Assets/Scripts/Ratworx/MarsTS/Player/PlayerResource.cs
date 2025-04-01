using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Player {

    public class PlayerResource : NetworkBehaviour {

        public string Key => key;

        [SerializeField]
        private string key;

        public int Amount {
            get => _storedResources.Value;
            private set => _storedResources.Value = value;
        }

        private readonly NetworkVariable<int> _storedResources = new(writePerm:NetworkVariableWritePermission.Server);

        [SerializeField]
        private int startingAmount;

        private EventAgent _bus;

        private Faction _player;

		private void Awake () {
            _bus = GetComponent<EventAgent>();
            _player = GetComponent<Faction>();
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
            _storedResources.OnValueChanged += OnResourceValueChange;
        }

        private void OnResourceValueChange (int oldAmount, int newAmount) {
			_bus.Global(new ResourceUpdateEvent(_bus, _player, this));
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