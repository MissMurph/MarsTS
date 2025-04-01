using System;

namespace Ratworx.MarsTS.Registry
{
    public interface IRegistry
    {
        event Action<string> OnRegistryLoaded;
        string Key { get; }
        string Namespace { get; }
    }
}