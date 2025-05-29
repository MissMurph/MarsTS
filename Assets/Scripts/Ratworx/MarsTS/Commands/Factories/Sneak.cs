using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories {

    public class Sneak : CommandFactory<bool> {
		public override string Name => "sneak";

		public override Type TargetType => typeof(bool);

		public override string Description => description;

		[SerializeField]
		private string description;

		public override void StartSelection () {
			int totalWithSneak = 0;
			int totalSneakActive = 0;

			//Inspect all selected to make all units using this ability match up with others that are active using
			foreach (Roster rollup in Player.Player.Selected.Values) {
				if (rollup.GetCommands().Contains(Name)) {
					totalWithSneak += rollup.Count;

					foreach (ICommandable unit in rollup.GetCommandables()) {
						if (unit.ActiveCommands.Count == 0) continue;

						foreach (ICommandReceiver activeCommand in unit.ActiveCommands) {
							if (activeCommand.CommandKey == Name) totalSneakActive++;
						}
					}
				}
			}

			Construct(totalWithSneak > totalSneakActive);
		}

		public void Construct(bool status) {
			ConstructCommandletServerRpc(
				status,
				Player.Player.Commander.Id,
				Player.Player.ListSelected.ToArray(),
				Player.Player.Include
			);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(
			bool status, 
			int factionId, 
			int[] selection, 
			bool inclusive
		) {
			ConstructCommandletServer(status, factionId, selection, inclusive);
		}

		public override ResourceCost[] GetCost () {
			return new ResourceCost[1] { new ResourceCost { key = "time", amount = 60} };
		}

		public override void CancelSelection () {
			
		}
	}
}