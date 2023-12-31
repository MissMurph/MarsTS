using MarsTS.UI;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

	public class Construct : Command<ISelectable> {

		public override string Name { get { return "construct"; } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		[SerializeField]
		private CommandPage buildingCommands;

		public override void StartSelection () {
			UIController.Command.LoadCommandPage(buildingCommands);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[0];
		}

		public override void CancelSelection () {
			
		}
	}
}