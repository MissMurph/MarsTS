namespace MarsTS.Prefabs
{
    public interface IRegistryObject<T> : IRegistryObject
    {
        T Get();
    }

    public interface IRegistryObject
    {
        string RegistryType { get; }
        string RegistryKey { get; }
    }
}