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

		private void Awake () {
            stored = startingAmount;
		}

        public bool Deposit (int amount) {
            stored += amount;
            return true;
        }

        public bool Withdraw (int amount) {
            if (Amount >= amount) {
                stored -= amount;
                return true;
            }
            else return false;
        }
	}
}