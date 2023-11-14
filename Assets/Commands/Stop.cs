using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public class Stop : Command {
		public override string Name { get { return "stop"; } }

		public override Type TargetType { get { return typeof(bool); } }

		public override void StartSelection () {
			Player.Main.DeliverCommand(new Commandlet<bool>(Name, true, Player.Main), Player.Include);
		}
	}
}