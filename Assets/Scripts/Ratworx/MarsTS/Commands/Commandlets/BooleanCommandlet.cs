using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Teams;

namespace Ratworx.MarsTS.Commands.Commandlets {

	public class BooleanCommandlet : Commandlet<bool> {
		public override string SerializerKey => "boolean";

		protected override ISerializedCommand Serialize() => CommandSerializers.Write("boolean", this);

		protected override void Deserialize (SerializedCommandWrapper data) {
			base.Deserialize(data);

			var deserialized = (SerializedBoolCommandlet)data.commandletData;
			
			Name = data.Name;
			Commander = TeamCache.Faction(data.Faction);
			_target = deserialized.Status;
		}
	}
}