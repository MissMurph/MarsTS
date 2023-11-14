using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players {

    public class PlayerResource : MonoBehaviour {

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

        public void Submit (int amount) {
            stored += amount;
        }

        public bool Consume (int amount) {
            if (Amount >= amount) {
                stored -= amount;
                return true;
            }
            else return false;
        }
	}
}