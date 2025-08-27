using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Registry
{
    public class NetworkPrefabPrimer : MonoBehaviour
    {
        private void Start()
        {
            bool isPriming = true;

            using IEnumerator<GameObject> networkPrefabs = Registry.GetAllPrefabs()
                .Select(kvp => kvp.Item2)
                .GetEnumerator();
            
            while(isPriming) {
                try {
                    while (networkPrefabs.MoveNext() && networkPrefabs.Current is not null) {
                         if (!networkPrefabs.Current.TryGetComponent<NetworkObject>(out _)) 
                             continue;
                        
                         NetworkManager.Singleton.AddNetworkPrefab(networkPrefabs.Current);
                         RatLogger.Verbose?.Log($"Registered {networkPrefabs.Current.name} as Network Prefab");
                    }

                    isPriming = false;
                }
                catch (Exception e) {
                    RatLogger.Error?.Log(e);
                }
            }
        }
    }
}