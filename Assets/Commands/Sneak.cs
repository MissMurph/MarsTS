using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class Sneak : Command {
		public override string Name { get { return "sneak"; } }

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		public override void StartSelection () {
			Player.Main.DeliverCommand(new Commandlet<bool>(Name, true, Player.Main), true);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = 60} };
		}

		public override void CancelSelection () {
			
		}
	}
}