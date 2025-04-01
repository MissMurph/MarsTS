using System;
using System.Collections.Generic;

namespace MarsTS.Prefabs
{
    public interface IRegistry
    {
        event Action<string> OnRegistryLoaded;
        string Key { get; }
        string Namespace { get; }
    }
}