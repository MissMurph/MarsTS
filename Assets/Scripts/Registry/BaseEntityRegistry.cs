using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Buildings;
using MarsTS.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Prefabs
{
    public abstract class BaseEntityRegistry<T> : MonoBehaviour, IPrefabRegistry, IObjectRegistry<T>
    {
        public event Action<string, GameObject> OnPrefabRegistered;
        public event Action<string, T> OnEntityRegistered;
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

        protected bool RegisterPrefabAndObject(string key, T registryObject, GameObject prefab)
        {
            if (_registeredPrefabs.ContainsKey(key) || _registeredObjects.ContainsKey(key))
                Debug.LogWarning(
                    $"Entity {key} of Type {typeof(T)} already registered with {Key}:{Namespace}! Overriding");

            _registeredPrefabs[key] = prefab;
            _registeredObjects[key] = registryObject;

            return true;
        }

        public bool RegisterPrefab(string key, GameObject prefab)
        {
            if (prefab.TryGetComponent(out T component))
                return RegisterPrefabAndObject(key, component, prefab);

            Debug.LogError($"Attempted to register Prefab {key} to registry of Type {typeof(T)}: {Key}:{Namespace}");
            return false;
        }

        public bool RegisterObject(string key, T registryObject)
        {
            if (registryObject is Component component)
                return RegisterPrefabAndObject(key, registryObject, component.gameObject);
            
            Debug.LogError($"Cannot register object {key} of Type {typeof(T)}! Object is not a component and no GameObject has been provided!");
            return false;
        }

        public bool TryGetPrefab(string key, out GameObject prefab) => _registeredPrefabs.TryGetValue(key, out prefab);

        public bool TryGetObject(string key, out T registryObject) =>
            _registeredObjects.TryGetValue(key, out registryObject);

        public List<GameObject> GetAllPrefabs() => _registeredPrefabs.Values.ToList();

        public List<T> GetAllObjects() => _registeredObjects.Values.ToList();
    }
}