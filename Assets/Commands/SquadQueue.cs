using MarsTS.Events;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class SquadQueue : CommandQueue {

		protected InfantrySquad parentSquad;

		protected override void Awake () {
			base.Awake();

			parentSquad = orderSource as InfantrySquad;
		}

		protected override void OnOrderComplete (CommandCompleteEvent _event) {
			if (!parentSquad.Members.Contains(_event.Unit as ISelectable)) return;
			Current = null;
			bus.Global(_event);
		}
	}
}