using MarsTS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units.Commands {

	public class Construct : Command<ISelectable> {

		public override string Name { get { return "construct"; } }

		[SerializeField]
		private CommandPage buildingCommands;

		public override void StartSelection () {
			UIController.Command.LoadCommandPage(buildingCommands);
		}
	}
}