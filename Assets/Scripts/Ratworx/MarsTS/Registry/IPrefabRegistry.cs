using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ratworx.MarsTS.Registry
{
    public interface IPrefabRegistry : IRegistry
    {
        event Action<string, GameObject> OnPrefabRegistered;
        bool RegisterPrefab(string key, GameObject prefab);
        bool TryGetPrefab(string key, out GameObject prefab);
        List<(string, GameObject)> GetAllPrefabs();
    }
}