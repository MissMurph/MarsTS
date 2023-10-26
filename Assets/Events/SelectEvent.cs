using MarsTS.Units;
using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MarsTS.Events {

	public class SelectEvent : AbstractEvent {

		public Dictionary<string, Roster> Selected { get; private set; }

		public SelectEvent (Dictionary<string, Roster> selection) : base("select", Player.EventAgent) {
			Selected = selection;
		}
	}
}