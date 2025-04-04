using System;
using MarsTS.Teams;

namespace MarsTS.Commands {

	public class SimpleCommandlet : Commandlet<bool> {
		public override string SerializerKey => "simple";

		public override Commandlet Clone () {
			throw new NotImplementedException();
		}

		protected override ISerializedCommand Serialize() => CommandSerializers.Write("simple", this);

		protected override void Deserialize (SerializedCommandWrapper data) {
			base.Deserialize(data);

			var deserialized = (SerializedBoolCommandlet)data.commandletData;
			
			Name = data.Name;
			Commander = TeamCache.Faction(data.Faction);
			_target = deserialized.Status;
		}
	}
}