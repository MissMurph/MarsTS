using System;
using MarsTS.UI;
using MarsTS.Units;
using UnityEngine;

namespace MarsTS.Commands {

	public class Construct : CommandFactory<ISelectable> {

		public override string Name => "construct";

		public override string Description => description;

		[SerializeField]
		private string description;

		[SerializeField]
		private CommandPage buildingCommands;

		public override void StartSelection () {
			UIController.Command.LoadCommandPage(buildingCommands);
		}

		public override CostEntry[] GetCost () => Array.Empty<CostEntry>();

		public override void CancelSelection () { }
	}
}