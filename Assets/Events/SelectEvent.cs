using MarsTS.Units;
using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MarsTS.Events {

	public class SelectEvent : AbstractEvent {

		public Dictionary<string, int> Selected { get; private set; }
		public Player Owner { get; private set; }
		public bool SelectStatus { get; private set; }

		public SelectEvent (Player player, bool selectStatus, params ISelectable[] units) : base("select") {
			Owner = player;
			Selected = new Dictionary<string, int>();

			AddUnits(units);

			SelectStatus = selectStatus;
		}

		public void AddUnits (params ISelectable[] units) {
			foreach (ISelectable unit in units) {
				int newTally = GetTally(unit.Type()) + 1;
				Selected[unit.Type()] = newTally;
			}
		}

		private int GetTally (string type) {
			int output = Selected.GetValueOrDefault(type, 0);
			if (!Selected.ContainsKey(type)) Selected.Add(type, output);
			return output;
		}
	}
}