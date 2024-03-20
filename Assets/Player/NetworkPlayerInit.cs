using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Players {

    public class NetworkPlayerInit : NetworkBehaviour {

        [SerializeField]
        private Player clientPrefab;

        public override void OnNetworkSpawn () {
            if (IsOwner) {
                Instantiate(clientPrefab, transform);
			}
        }
    }
}