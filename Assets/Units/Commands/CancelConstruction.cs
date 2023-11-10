using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MarsTS.Units.Commands {

	public class CancelConstruction : Command<bool> {

		public override string Name { get { return "cancelConstruction"; } }

		public override void StartSelection () {
			Player.Main.DeliverCommand(Construct(true), Player.Include);
		}
	}
}