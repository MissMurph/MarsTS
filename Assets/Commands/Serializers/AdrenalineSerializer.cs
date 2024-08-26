using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {
    public class AdrenalineSerializer : MonoBehaviour, ICommandSerializer {
        public string Key => commandKey;
        
        [SerializeField]
        private string commandKey;
        
        public ISerializedCommand Reader() =>
            new SerializedAdrenalineCommandlet {
                Key = Key
            };

        public ISerializedCommand Writer(Commandlet _data) {
            AdrenalineCommandlet superType = _data as AdrenalineCommandlet;

            return new SerializedAdrenalineCommandlet {
                Key = Key,
                Faction = superType.Commander.Id,
                Id = superType.Id,
                Status = superType.Target
            };
        }
    }

    public struct SerializedAdrenalineCommandlet : ISerializedCommand {
        public string Key { get; set; }
        public int Faction { get; set; }
        public int Id { get; set; }

        public bool Status;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref Status);
        }
    }
}