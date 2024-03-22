using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public class Stop : CommandFactory {
		public override string Name { get { return "stop"; } }

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		public override void StartSelection () {
			//Player.Main.DeliverCommand(new Commandlet<bool>(Name, true, Player.Commander), Player.Include);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[0];
		}

		public override void CancelSelection () {
			
		}
	}
}