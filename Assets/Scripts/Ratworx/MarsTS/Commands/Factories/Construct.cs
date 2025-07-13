using System;
using Ratworx.MarsTS.Player;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories {

	public class Construct : CommandFactory<ISelectable> {

		public override string Name => "construct";

		// TODO: Revisit below (command pages)
		
		[Rpc(SendTo.Server)]
		protected override void ConstructCommandServerRpc(string commandKey, ISelectable target, int factionId, int[] selection, bool enqueue)
			=> ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
		
		/*public override string Description => description;

		[SerializeField]
		private string description;

		[SerializeField]
		private CommandPage buildingCommands;

		public override void StartSelection () {
			UIController.Command.LoadCommandPage(buildingCommands);
		}

		public override ResourceCost[] GetCost () => Array.Empty<ResourceCost>();

		public override void CancelSelection () { }*/
	}
}