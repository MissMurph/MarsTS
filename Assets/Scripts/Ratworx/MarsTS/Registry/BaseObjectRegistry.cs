using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Logging;
using UnityEngine;

namespace Ratworx.MarsTS.Registry
{
    public abstract class BaseObjectRegistry<T> : MonoBehaviour, 
        IPrefabRegistry, 
        IObjectRegistry<T>
        where T : IRegistryObject<T>
    {
        public event Action<string, GameObject> OnPrefabRegistered;
        public event Action<string, T> OnObjectRegistered;
        public event Action<string> OnRegistryLoaded;
        public string Key => _key;
        public string Namespace => _namespace;

        [SerializeField] private string _key;
        [SerializeField] private string _namespace;
        [SerializeField] private GameObject[] _prefabsToRegister;

        private Dictionary<string, GameObject> _registeredPrefabs;
        private Dictionary<string, T> _registeredObjects;

        private void Awake()
        {
            _registeredPrefabs = new Dictionary<string, GameObject>();
            _registeredObjects = new Dictionary<string, T>();

            foreach (GameObject prefab in _prefabsToRegister)
            {
                if (!prefab.TryGetComponent(out T component)) 
                    continue;
                
                RegisterPrefabAndObject(prefab.name, component, prefab);
            }
        }

        private bool RegisterPrefabAndObject(string key, T registryObject, GameObject prefab)
        {
            string casedKey = registryObject.RegistryKey.ToLower();
            
            if (_registeredPrefabs.ContainsKey(casedKey) || _registeredObjects.ContainsKey(casedKey))
                RatLogger.Error?.Log(
                    $"Entity {casedKey} of Type {typeof(T)} already registered with {Key}:{Namespace}! Overriding");

            _registeredPrefabs[casedKey] = prefab;
            _registeredObjects[casedKey] = registryObject;
            
            RatLogger.Verbose?.Log($"Registered Object of type {typeof(T)} {Namespace}:{_key}:{casedKey}");

            return true;
        }

        public bool RegisterPrefab(string key, GameObject prefab)
        {
            if (prefab.TryGetComponent(out T component))
                return RegisterPrefabAndObject(component.RegistryKey, component, prefab);

            RatLogger.Error?.Log($"Attempted to register Prefab {key} to registry of Type {typeof(T)}: {Key}:{Namespace}");
            return false;
        }

        public bool RegisterObject(string key, T registryObject)
        {
            if (registryObject is Component component)
                return RegisterPrefabAndObject(registryObject.RegistryKey, registryObject, component.gameObject);
            
            RatLogger.Error?.Log($"Cannot register object {key} of Type {typeof(T)}! Object is not a component and no GameObject has been provided!");
            return false;
        }

        public bool TryGetPrefab(string key, out GameObject prefab) => _registeredPrefabs.TryGetValue(key, out prefab);

        public bool TryGetObject(string key, out T registryObject) =>
            _registeredObjects.TryGetValue(key, out registryObject);

        public List<(string, GameObject)> GetAllPrefabs() => _registeredPrefabs
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();

        public List<T> GetAllObjects() => _registeredObjects.Values.ToList();
    }
}