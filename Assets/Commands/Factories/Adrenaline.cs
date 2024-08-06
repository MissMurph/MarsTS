using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class Adrenaline : CommandFactory<bool> {

		public override string Name { get { return commandName; } }

		[SerializeField]
		private string commandName;

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		[SerializeField]
		private float duration;

		[SerializeField]
		private float cooldown;

		public override void StartSelection () {
			int totalCanUse = 0;
			int totalUsing = 0;

			//Inspect all selected to make all units using this ability match up with others that are active using
			foreach (Roster rollup in Player.Selected.Values) {
				if (rollup.Commands.Contains(Name)) {
					foreach (ICommandable unit in rollup.Orderable) {
						if (unit.CanCommand(Name)) totalCanUse++;

						if (unit.Active.Count == 0) continue;

						foreach (string activeCommand in unit.Active) {
							if (activeCommand == Name) totalUsing++;
						}
					}
				}
			}

			//Player.Main.DeliverCommand(Construct(totalCanUse > totalUsing), Player.Include);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = (int)cooldown } };
		}

		public override void CancelSelection () {

		}
	}

	public class AdrenalineCommandlet : Commandlet<bool> {

		private float duration;
		//private float remainingDuration;

		private float cooldown;

		public AdrenalineCommandlet (string _name, float _duration, float _cooldown, bool _status) {
			duration = _duration;
			//remainingDuration = _duration;
			cooldown = _cooldown;
		}

		public override string Key => Name;

		public override void ActivateCommand (CommandQueue queue, CommandActiveEvent _event) {
			if (_event.Activity) {
				queue.Cooldown(this, duration);
			}
			else {
				queue.Cooldown(this, cooldown);
			}
		}
	}
}