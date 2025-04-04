namespace Ratworx.MarsTS.Registry
{
    public interface IRegistryObject<T> : IRegistryObject
    {
        T GetEntityComponent();
    }

    public interface IRegistryObject
    {
        string RegistryType { get; }
        string RegistryKey { get; }
    }
}