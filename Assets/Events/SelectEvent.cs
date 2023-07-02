using MarsTS.Units;
using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MarsTS.Events {

	public class SelectEvent : AbstractEvent {

		public Dictionary<string, Roster> Selected { get; private set; }
		public Player Owner { get; private set; }
		public bool SelectStatus { get; private set; }

		public SelectEvent (Player player, bool selectStatus, Dictionary<string, Roster> selection) : base("select") {
			Owner = player;
			Selected = selection;
			SelectStatus = selectStatus;
		}
	}
}