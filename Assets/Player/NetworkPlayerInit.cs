using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Players {

    public class NetworkPlayerInit : NetworkBehaviour {

        [SerializeField]
        private Player clientPrefab;

        private Faction _faction;
        
        private void Awake() {
            _faction = GetComponent<Faction>();
        }

        public override void OnNetworkSpawn () {
            if (IsOwner) {
                Instantiate(clientPrefab, null)
                    .GetComponent<Player>()
                    .SetCommander(_faction);
			}
        }
    }
}