using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Prefabs
{
    public interface IPrefabRegistry : IRegistry
    {
        event Action<string, GameObject> OnPrefabRegistered;
        bool RegisterPrefab(string key, GameObject prefab);
        bool TryGetPrefab(string key, out GameObject prefab);
        List<GameObject> GetAllPrefabs();
    }
}