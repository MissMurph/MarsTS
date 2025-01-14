using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsTS.Prefabs
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

            foreach (GameObject prefab in _prefabsToRegister)
            {
                RegisterPrefab(prefab.name, prefab);
            }
            
            OnRegistryLoaded?.Invoke($"{Key}:{Namespace}");
        }

        public bool RegisterPrefab(string key, GameObject prefab)
        {
            if (_registeredPrefabs.ContainsKey(key))
            {
                Debug.LogWarning($"Prefab {key} already registered with {Key}:{Namespace}! Overriding");
            }

            _registeredPrefabs[key] = prefab;

            return true;
        }

        public bool TryGetPrefab(string key, out GameObject prefab) => _registeredPrefabs.TryGetValue(key, out prefab);

        public List<GameObject> GetAllPrefabs() => _registeredPrefabs.Values.ToList();
    }
}