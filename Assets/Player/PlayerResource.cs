using MarsTS.Events;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players {

    public class PlayerResource : MonoBehaviour {

        public string Key { get { return key; } }

        [SerializeField]
        private string key;

        public int Amount {
            get {
                return stored;
            }
        }

        private int stored;

        [SerializeField]
        private int startingAmount;

        private EventAgent bus;

        private Faction player;

		private void Awake () {
            stored = startingAmount;
            bus = GetComponent<EventAgent>();
            player = GetComponent<Faction>();
		}

        public bool Deposit (int amount) {
            stored += amount;
            bus.Global(new ResourceUpdateEvent(bus, player, this));
            return true;
        }

        public bool Withdraw (int amount) {
            if (Amount >= amount) {
                stored -= amount;
				bus.Global(new ResourceUpdateEvent(bus, player, this));
				return true;
            }
            else return false;
        }
	}
}