using System;
using MarsTS.Teams;

namespace MarsTS.Commands {

	public class SimpleCommandlet : Commandlet<bool> {

		public override string Key => Name;

		public override Commandlet Clone () {
			throw new NotImplementedException();
		}

		protected override ISerializedCommand Serialize() => CommandSerializers.Write("simple", this);

		protected override void Deserialize (SerializedCommandWrapper data) {
			Name = data.Key;
			Commander = TeamCache.Faction(data.Faction);
		}
	}
}