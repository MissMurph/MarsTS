using Unity.Netcode;

namespace Ratworx.MarsTS.Commands {

	public interface ISerializedCommand : INetworkSerializable {
		string Name { get;  }
		string SerializerKey { get; }
		int Faction { get; }
		int Id { get; }
	}
}