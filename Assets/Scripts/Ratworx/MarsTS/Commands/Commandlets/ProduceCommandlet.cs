using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Commands.Serializers;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Commandlets {

    public class ProduceCommandlet : Commandlet<string> 
	{
		public override string SerializerKey => commandKey;
		
		public Dictionary<string, int> Cost { get; private set; }
		public override CommandFactory Command => CommandPrimer.Get(commandKey);

		private string commandKey;

		public string ProductRegistryKey { get; private set; }

		public void InitProduce (string name, string commandKey, string productRegistryKey, Faction commander) {
			ProductRegistryKey = productRegistryKey;

			this.commandKey = commandKey;

			//Calling the rest of the Init will also spawn & sync the commandlet, make sure all data is created
			//BEFORE the sync
			Init(commandKey, productRegistryKey, commander);
		}
		
		protected override void Deserialize (SerializedCommandWrapper data) {
			base.Deserialize(data);
			var deserialized = (SerializedProduceCommandlet)data.commandletData;
			_target = deserialized.ProductRegistryKey;
		}
	}
}