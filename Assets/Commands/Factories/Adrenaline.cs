using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System;
using System.Collections.Generic;
using MarsTS.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

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
			foreach (Roster rollup in Player.Selected.Values) {
				if (!rollup.Commands.Contains(Name)) continue;

				foreach (ICommandable unit in rollup.Orderable) {
					if (unit.CanCommand(Name)) totalCanUse++;

					if (unit.Active.Count == 0) continue;

					foreach (string activeCommand in unit.Active) {
						if (activeCommand == Name) totalUsing++;
					}
				}
			}

			Construct(totalCanUse > totalUsing, Player.ListSelected);
		}

		public override CostEntry[] GetCost () 
			=> new CostEntry[1] { new CostEntry { key = "time", amount = (int)cooldown } };

		public override void CancelSelection () {

		}
		
		public void Construct(bool status, List<string> selection) {
			ConstructCommandletServerRpc(status, Player.Commander.Id, selection.ToNativeArray32(), Player.Include);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(bool status, int factionId, NativeArray<FixedString32Bytes> selection, bool inclusive) {
			ConstructCommandletServer(status, factionId, selection.ToStringList(), inclusive);
		}
	}
}