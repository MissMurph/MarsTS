using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class Sneak : CommandFactory<bool> {
		public override string Name { get { return "sneak"; } }

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		[SerializeField]
		private float deactivateCooldown;

		[SerializeField]
		private float reactivateCooldown;

		public override void StartSelection () {
			int totalWithSneak = 0;
			int totalSneakActive = 0;

			//Inspect all selected to make all units using this ability match up with others that are active using
			foreach (Roster rollup in Player.Selected.Values) {
				if (rollup.Commands.Contains(Name)) {
					totalWithSneak += rollup.Count;

					foreach (ICommandable unit in rollup.Orderable) {
						if (unit.Active.Count == 0) continue;

						foreach (string activeCommand in unit.Active) {
							if (activeCommand == Name) totalSneakActive++;
						}
					}
				}
			}

			//Player.Main.DeliverCommand(Construct(totalWithSneak > totalSneakActive), true);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = 60} };
		}

		public override void CancelSelection () {
			
		}

		/*public override Commandlet Construct (bool target) {
			return new SneakCommandlet(Name, target, Player.Commander, deactivateCooldown, reactivateCooldown);
		}*/

		private class SneakCommandlet : Commandlet<bool> {

			private float deactivateCooldown;
			private float reactivateCooldown;

			public SneakCommandlet (string name, bool target, Faction commander, float _deactivateCooldown, float _reactivateCooldown) {
				deactivateCooldown = _deactivateCooldown;
				reactivateCooldown = _reactivateCooldown;
			}

			public override string Key => Name;

			public override void OnActivate (CommandQueue queue, CommandActiveEvent _event) {
				base.OnActivate(queue, _event);

				if (_event.Activity) queue.Cooldown(this, deactivateCooldown);
				else queue.Cooldown(this, reactivateCooldown);
			}
		}
	}
}