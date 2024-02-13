using MarsTS.Units;
using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MarsTS.Events {

	public class PlayerSelectEvent : AbstractEvent {

		public Dictionary<string, Roster> Selected { get; private set; }

		public PlayerSelectEvent (Dictionary<string, Roster> selection) : base("playerSelect", Player.EventAgent) {
			Selected = selection;
		}
	}
}