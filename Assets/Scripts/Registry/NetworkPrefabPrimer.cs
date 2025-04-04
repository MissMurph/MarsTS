using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Prefabs
{
    public class NetworkPrefabPrimer : MonoBehaviour
    {
        private void Start()
        {
            List<GameObject> networkPrefabs = Registry.GetAllPrefabs()
                .Where(kvp => kvp.Item2.TryGetComponent<NetworkObject>(out _))
                .Select(kvp => kvp.Item2)
                .ToList();

            foreach (GameObject prefab in networkPrefabs)
            {
                NetworkManager.Singleton.AddNetworkPrefab(prefab);
            }
        }
    }
}