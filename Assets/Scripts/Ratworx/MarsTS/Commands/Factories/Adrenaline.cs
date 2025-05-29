using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Units;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories {

    public class Adrenaline : CommandFactory<bool> {

		public override string Name => commandName;

		[SerializeField]
		private string commandName;

		public override Type TargetType => typeof(bool);

		public override string Description => description;

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
			foreach (Roster roster in Player.Player.Selected.Values) {
				if (!roster.GetCommands().Contains(Name)) continue;

				foreach (ICommandable unit in roster.GetCommandables()) {
					if (unit.CanCommand(Name)) totalCanUse++;

					if (unit.ActiveCommands.Count == 0) continue;

					foreach (ICommandReceiver activeCommand in unit.ActiveCommands) {
						if (activeCommand.CommandKey == Name) totalUsing++;
					}
				}
			}

			Construct(totalCanUse > totalUsing, Player.Player.ListSelected);
		}

		public override ResourceCost[] GetCost () 
			=> new ResourceCost[1] { new ResourceCost { key = "time", amount = (int)cooldown } };

		public override void CancelSelection () {

		}
		
		public void Construct(bool status, List<int> selection) {
			ConstructCommandletServerRpc(status, Player.Player.Commander.Id, selection.ToArray(), Player.Player.Include);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(bool status, int factionId, int[] selection, bool inclusive) {
			ConstructCommandletServer(status, factionId, selection, inclusive);
		}
	}
}