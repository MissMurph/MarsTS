using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Commands
{
    public class HarvestableSerializer : MonoBehaviour, ICommandSerializer
    {
        public string Key => _commandKey;

        [SerializeField] private string _commandKey;

        public ISerializedCommand Reader()
        {
            return new SerializedHarvestableCommandlet
            {
                SerializerKey = Key
            };
        }

        public ISerializedCommand Writer(Commandlet data)
        {
            if (data is null) {
                Debug.LogError($"Commandlet cannot be serialized by {typeof(HarvestableSerializer)}:{Key} because Data is null!");
                return null;
            }
            
            HarvestableCommandlet superType = data as HarvestableCommandlet;

            return new SerializedHarvestableCommandlet
            {
                Name = data.Name,
                SerializerKey = Key,
                Faction = superType.Commander.Id,
                Id = superType.Id,
                TargetUnit = superType.Target.GameObject.name,
            };
        }
    }

    public struct SerializedHarvestableCommandlet : ISerializedCommand
    {
        public string Name { get; set; }
        public string SerializerKey { get; set;  }
        public int Faction { get; set; }
        public int Id { get; set; }
        public string TargetUnit;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TargetUnit);
        }
    }
}