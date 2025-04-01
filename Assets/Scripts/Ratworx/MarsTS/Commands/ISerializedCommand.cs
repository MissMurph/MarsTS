using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public interface ISerializedCommand : INetworkSerializable {
		string Name { get;  }
		string SerializerKey { get; }
		int Faction { get; }
		int Id { get; }
	}
}