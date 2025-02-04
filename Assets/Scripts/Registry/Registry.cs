using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs
{
    public class Registry : MonoBehaviour
    {
        private static Registry _instance;

        private Dictionary<string, IRegistry> _registries;

        private int _loadedRegistries = 0;
        private int _registriesToLoad;

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log($"Registry Awake");

            _registries = new Dictionary<string, IRegistry>();

            IRegistry[] childRegistries = GetComponentsInChildren<IRegistry>();
            _registriesToLoad = childRegistries.Length;
            
            // TODO: Post Registry initialization pre event for collection
            
            foreach (IRegistry registry in childRegistries)
            {
                _registries[$"{registry.Key}"] = registry;
                registry.OnRegistryLoaded += OnRegistryLoaded;
            }
        }

        private void OnRegistryLoaded(string registryKey)
        {
            _loadedRegistries++;

            if (_loadedRegistries >= _registriesToLoad) ;
            // TODO: Post registry complete event
        }

        public static bool TryGet<T>(string key, out T registryObject)
        {
            registryObject = default;
            
            string[] split = key.Split(':');

            if (!(split.Length > 1))
            {
                Debug.LogError($"Invalid registry key: {key}");
                return false;
            }

            string registryType = split[0];
            string objectType = split[1];

            if (_instance._registries.TryGetValue(registryType, out IRegistry registry)
                && registry is IObjectRegistry<T> objectRegistry)
                return objectRegistry.TryGetObject(objectType, out registryObject);

            Debug.LogWarning($"Registry Object {key} of Type {typeof(T)} not found!");
            return false;
        }

        public static bool TryGetPrefab(string key, out GameObject prefab)
        {
            prefab = null;
            
            string[] split = key.Split(':');

            if (!(split.Length > 1))
            {
                Debug.LogWarning($"Invalid registry key: {key}");
                return false;
            }

            string registryType = split[0];
            string objectType = split[1];

            if (_instance._registries.TryGetValue(registryType, out IRegistry registry)
                && registry is IPrefabRegistry prefabRegistry)
                return prefabRegistry.TryGetPrefab(objectType, out prefab);

            Debug.LogWarning($"Registry Object {key} not found!");
            return false;
        }

        public static List<GameObject> GetAllPrefabs()
        {
            if (_instance == null) return null;

            var prefabs = new List<GameObject>();
            
            foreach (IRegistry registry in _instance._registries.Values)
            {
                if (registry is IPrefabRegistry prefabRegistry) 
                    prefabs.AddRange(prefabRegistry.GetAllPrefabs());
            }

            return prefabs;
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}