using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories {

	public class Deploy : CommandFactory<bool> {
		public override string Name => commandName;

		[SerializeField]
		private string commandName;

		public override Type TargetType => typeof(bool);

		public override string Description => description;

		[SerializeField]
		private string description;

		[SerializeField]
		private int deployTime;

		public override void StartSelection () {
			var toCommand = new List<int>();

			foreach (Roster rollup in Player.Player.Selected.Values) {
				if (!rollup.GetCommands().Contains(Name)) 
					continue;
				
				foreach (ICommandable unit in rollup.GetCommandables()) {
					if (unit.ActiveCommands.Count == 0) continue;
					if (unit.Commands()[Name].IsActive) continue;

					toCommand.Add(unit.Entity.Id);
				}
			}

			Construct(toCommand);
		}

		// This can only be done per unit
		public void Construct(List<int> selection) {
			ConstructCommandletServerRpc(Player.Player.Commander.Id, selection.ToArray(), Player.Player.Include);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(int factionId, int[] selection, bool inclusive) {
			ConstructCommandServer(true, factionId, selection, inclusive);
		}

		public override ResourceCost[] GetCost () {
			return new ResourceCost[1] { new ResourceCost { key = "time", amount = 5 } };
		}

		public override void CancelSelection () {

		}
	}

	public interface IWorkable {
		int WorkRequired { get; }
		int CurrentWork { get; set; }
		public event Action<int, int> OnWork;
	}
}