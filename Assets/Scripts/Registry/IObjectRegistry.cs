using System;
using System.Collections.Generic;

namespace MarsTS.Prefabs
{
    public interface IObjectRegistry<T> : IRegistry
    {
        event Action<string, T> OnObjectRegistered;
        bool RegisterObject(string key, T registryObject);
        bool TryGetObject(string key, out T registryObject);
        List<T> GetAllObjects();
    }
}