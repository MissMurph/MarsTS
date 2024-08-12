using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

    public class ProduceSerializer : MonoBehaviour, ICommandSerializer {

        public string Key { get { return commandKey; } }

		[SerializeField]
		private string commandKey;

        public ISerializedCommand Reader () {
			return new SerializedProduceCommandlet {
				Key = Key
			};

		}

        public ISerializedCommand Writer (Commandlet _data) {
			ProduceCommandlet superType = _data as ProduceCommandlet;

			return new SerializedProduceCommandlet {
				Key = Key,
				Faction = superType.Commander.Id,
				Id = superType.Id,
				_prefabKey = "unit:" + superType.Product.name,
				_productionRequired = superType.ProductionRequired
			};
        }
    }

	public struct SerializedProduceCommandlet : ISerializedCommand {

		public string Key { get; set; }
		public int Faction { get; set; }
		public int Id { get; set; }

		public int _productionRequired;
		public string _prefabKey;

		public void NetworkSerialize<T> (BufferSerializer<T> serializer) where T : IReaderWriter {
			serializer.SerializeValue(ref _productionRequired);
			serializer.SerializeValue(ref _prefabKey);
		}
	}
}