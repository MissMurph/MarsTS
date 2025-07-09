using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Logging;
using UnityEngine;

namespace Ratworx.MarsTS.Registry
{
    public class BasePrefabRegistry : MonoBehaviour, IPrefabRegistry
    {
        public event Action<string, GameObject> OnPrefabRegistered;
        public event Action<string> OnRegistryLoaded;

        public string Key => _key;
        public string Namespace => _namespace;
        
        [SerializeField] private string _key;
        [SerializeField] private string _namespace;
        [SerializeField] private GameObject[] _prefabsToRegister;
        
        private Dictionary<string, GameObject> _registeredPrefabs;
        
        private void Awake()
        {
            _registeredPrefabs = new Dictionary<string, GameObject>();

            bool isRegistering = true;

            using IEnumerator<GameObject> prefabCache = _prefabsToRegister.AsEnumerable().GetEnumerator();
            while(isRegistering)
            {
                try {
                    while (prefabCache.MoveNext() && prefabCache.Current is not null) {
                        RegisterPrefab(prefabCache.Current.name, prefabCache.Current);
                    }

                    isRegistering = false;
                }
                catch (Exception e) {
                    RatLogger.Error?.Log(e);
                }
            }
            
            OnRegistryLoaded?.Invoke($"{Key}:{Namespace}");
        }

        public bool RegisterPrefab(string key, GameObject prefab)
        {
            string casedKey = key.ToLower();
            
            if (_registeredPrefabs.ContainsKey(casedKey)) 
                Debug.LogWarning($"Prefab {casedKey} already registered with {Key}:{Namespace}! Overriding");

            _registeredPrefabs[casedKey] = prefab;
            
            RatLogger.Verbose?.Log($"Registered prefab of {Namespace}:{_key}:{casedKey}");

            return true;
        }

        public bool TryGetPrefab(string key, out GameObject prefab) => _registeredPrefabs.TryGetValue(key, out prefab);

        public List<(string, GameObject)> GetAllPrefabs() => _registeredPrefabs
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }
}