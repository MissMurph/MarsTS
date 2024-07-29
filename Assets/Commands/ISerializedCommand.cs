using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public interface ISerializedCommand : INetworkSerializable {
		string Key { get; }
		int Faction { get; }
		int Id { get; }
	}
}