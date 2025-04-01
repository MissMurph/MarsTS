using Ratworx.MarsTS.Commands.Commandlets;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Serializers
{
    public class AdrenalineSerializer : MonoBehaviour, ICommandSerializer
    {
        public string Key => commandKey;

        [SerializeField] private string commandKey;

        public ISerializedCommand Reader()
            => new SerializedAdrenalineCommandlet
            {
                SerializerKey = Key
            };

        public ISerializedCommand Writer(Commandlet data)
        {
            AdrenalineCommandlet superType = data as AdrenalineCommandlet;

            return new SerializedAdrenalineCommandlet
            {
                Name = data.Name,
                SerializerKey = Key,
                Faction = superType.Commander.Id,
                Id = superType.Id,
                Status = superType.Target
            };
        }
    }

    public struct SerializedAdrenalineCommandlet : ISerializedCommand
    {
        public string Name { get; set; }
        public string SerializerKey { get; set; }
        public int Faction { get; set; }
        public int Id { get; set; }

        public bool Status;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Status);
        }
    }
}