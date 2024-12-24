using System;
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
                Key = Key
            };

        public ISerializedCommand Writer(Commandlet data)
        {
            ProduceCommandlet superType = data as ProduceCommandlet;

            return new SerializedProduceCommandlet
            {
                Key = Key,
                Faction = superType.Commander.Id,
                Id = superType.Id,
                PrefabKey = "unit:" + superType.Product.name,
                ProductionRequired = superType.ProductionRequired
            };
        }
    }

    public struct SerializedProduceCommandlet : ISerializedCommand
    {
        public string Key { get; set; }
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