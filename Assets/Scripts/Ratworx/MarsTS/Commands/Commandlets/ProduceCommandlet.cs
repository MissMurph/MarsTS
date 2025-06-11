using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;

namespace Ratworx.MarsTS.Commands.Commandlets {

    public class ProduceCommandlet : Commandlet<ProductionOption>
	{
		public override string SerializerKey => "produce";

		protected override void Deserialize (SerializedCommandWrapper data) {
			base.Deserialize(data);
			var deserialized = (SerializedProduceCommandlet)data.commandletData;
			_target = deserialized.ProductionOption.GetDeserializedOption();
		}
	}
}