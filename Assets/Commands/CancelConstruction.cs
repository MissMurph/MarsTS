using MarsTS.Players;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MarsTS.Commands {

	public class CancelConstruction : Command<bool> {

		public override string Name { get { return "cancelConstruction"; } }

		public override Sprite Icon { get { return icon; } }

		[SerializeField]
		private Sprite icon;

		public override void StartSelection () {
			Player.Main.DeliverCommand(Construct(true), false);
		}
	}
}