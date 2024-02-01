using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class Sneak : Command {
		public override string Name { get { return "sneak"; } }

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		public override void StartSelection () {
			int totalWithSneak = 0;
			int totalSneakActive = 0;

			//Inspect all selected to make all units using this ability match up with others that are active using
			foreach (Roster rollup in Player.Selected.Values) {
				if (rollup.Commands.Contains(Name)) {
					totalWithSneak += rollup.Count;

					foreach (ICommandable unit in rollup.Orderable) {
						if (unit.Active.Length == 0) continue;

						foreach (string activeCommand in unit.Active) {
							if (activeCommand == Name) totalSneakActive++;
						}
					}
				}
			}

			Player.Main.DeliverCommand(new Commandlet<bool>(Name, totalWithSneak > totalSneakActive, Player.Main), true);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = 60} };
		}

		public override void CancelSelection () {
			
		}
	}
}