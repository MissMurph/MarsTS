using System;
using MarsTS.Prefabs;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands
{
    //TODO: Refactor this this sucks
    //Like it doesn't actually but is a lot of boilerplate
    public class ProduceSerializer : MonoBehaviour, ICommandSerializer
    {
        public string Key => _commandKey;

        [SerializeField] private string _commandKey;

        public ISerializedCommand Reader()
            => new SerializedProduceCommandlet
            {
                SerializerKey = Key
            };

        public ISerializedCommand Writer(Commandlet data) {
            if (data is not ProduceCommandlet superType) 
                return null;

            string prefabKey = superType.ProductRegistryKey;
            
            if (superType.Product.TryGetComponent(out IRegistryObject registryObject)) 
                prefabKey = $"{registryObject.RegistryType}:{registryObject.RegistryKey}";

            return new SerializedProduceCommandlet
            {
                Name = data.Name,
                SerializerKey = Key,
                Faction = superType.Commander.Id,
                Id = superType.Id,
                PrefabKey = prefabKey,
                ProductionRequired = superType.ProductionRequired
            };
        }
    }

    public struct SerializedProduceCommandlet : ISerializedCommand
    {
        public string Name { get; set; }
        public string SerializerKey { get; set; }
        public int Faction { get; set; }
        public int Id { get; set; }

        public int ProductionRequired;
        public string PrefabKey;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ProductionRequired);
            serializer.SerializeValue(ref PrefabKey);
        }
    }
}