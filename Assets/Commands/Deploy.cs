using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

	public class Deploy : Command {
		public override string Name { get { return commandName; } }

		[SerializeField]
		private string commandName;

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		public override void StartSelection () {
			Player.Main.DeliverCommand(new Commandlet<bool>(Name, true, Player.Main), Player.Include);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = 5 } };
		}

		public override void CancelSelection () {

		}
	}
}