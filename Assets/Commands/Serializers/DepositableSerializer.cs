using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands
{
    public class DepositableSerializer : MonoBehaviour, ICommandSerializer
    {
        public string Key => _commandKey;

        [SerializeField] private string _commandKey;

        public ISerializedCommand Reader()
        {
            return new SerializedDepositableCommandlet
            {
                SerializerKey = Key
            };
        }

        public ISerializedCommand Writer(Commandlet data)
        {
            if (data is null) {
                Debug.LogError($"Commandlet cannot be serialized by {typeof(DepositableSerializer)}:{Key} because Data is null!");
                return null;
            }
            
            DepositableCommandlet superType = data as DepositableCommandlet;

            return new SerializedDepositableCommandlet
            {
                Name = data.Name,
                SerializerKey = Key,
                Faction = superType.Commander.Id,
                Id = superType.Id,
                TargetUnit = superType.Target.GameObject.name,
            };
        }
    }

    public struct SerializedDepositableCommandlet : ISerializedCommand
    {
        public string Name { get; set; }
        public string SerializerKey { get; set; }
        public int Faction { get; set; }
        public int Id { get; set; }
        public string TargetUnit;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref TargetUnit);
        }
    }
}