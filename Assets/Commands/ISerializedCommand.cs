using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public interface ISerializedCommand : INetworkSerializable {
		public string Key { get; }
		public int Faction { get; }
	}
}