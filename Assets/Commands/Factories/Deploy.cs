using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Entities;
using MarsTS.Teams;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

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
			var toCommand = new List<ICommandable>();

			foreach (Roster rollup in Player.Selected.Values) {
				if (!rollup.Commands.Contains(Name)) 
					continue;
				
				foreach (ICommandable unit in rollup.Orderable) {
					if (unit.Active.Contains(Name))
						continue;
						
					toCommand.Add(unit);
				}
			}

			foreach (ICommandable unit in toCommand) {
				Construct(unit.GameObject.name);
			}
		}

		// This can only be done per unit
		public void Construct(string selection) {
			ConstructCommandletServerRpc(Player.Commander.Id, selection, Player.Include);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(int factionId, string selection, bool inclusive) {
			ConstructCommandletServer(true, factionId, new List<string>{ selection }, inclusive);
		}

		protected override void ConstructCommandletServer(bool target, int factionId, ICollection<string> selection, bool inclusive) {
			DeployCommandlet order = (DeployCommandlet)Instantiate(orderPrefab);

			order.InitDeploy(Name, target, TeamCache.Faction(factionId), deployTime);

			foreach (string entity in selection) {
				if (EntityCache.TryGet(entity, out ICommandable unit))
					unit.Order(order, inclusive);
				else
					Debug.LogWarning($"ICommandable on Unit {entity} not found! Command {Name} being ignored by unit!");
			}
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = 5 } };
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