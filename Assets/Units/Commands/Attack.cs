using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Units.Commands {

	public class Attack : Command<ISelectable> {
		public override string Name { get { return "attack"; } }

		public ISelectable Target { get; private set; }

		public override void StartSelection () {
			throw new NotImplementedException();
		}
	}
}