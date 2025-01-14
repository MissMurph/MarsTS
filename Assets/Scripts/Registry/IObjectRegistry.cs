using System;
using System.Collections.Generic;

namespace MarsTS.Prefabs
{
    public interface IObjectRegistry<T> : IRegistry
    {
        event Action<string, T> OnEntityRegistered;
        bool RegisterObject(string key, T registryObject);
        bool TryGetObject(string key, out T registryObject);
        List<T> GetAllObjects();
    }
}