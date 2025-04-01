using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Entities;
using MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public class Undeploy : CommandFactory<bool> {
		public override string Name { get { return commandName; } }

		[SerializeField]
		private string commandName;

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

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
					if (!unit.Active.Contains(Name))
						continue;
						
					toCommand.Add(unit);
				}
			}

			foreach (ICommandable unit in toCommand) {
				Construct(unit.GameObject.name);
			}
		}

		public void Construct(string selection) {
			
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

	/*public class UndeployCommandlet : Commandlet<bool>, IWorkable {

		public float WorkRequired { get; private set; }
		public float CurrentWork { get; set; }

		public UndeployCommandlet (string _name, bool _status, float _workRequired) {
			WorkRequired = _workRequired;
			CurrentWork = 0f;
		}

		public override void StartCommand (EventAgent agent, ICommandable unit) {
			base.StartCommand(agent, unit);

			if (TryGetQueue(unit, out var queue))
				queue.Cooldown(this, WorkRequired);
		}

		public override void CompleteCommand (EventAgent agent, ICommandable unit, bool isCancelled = false) {
			if (TryGetQueue(unit, out var queue))
				queue.Deactivate("deploy");
			
			base.CompleteCommand(agent, unit, isCancelled);
		}

		public override bool CanInterrupt () {
			return false;
		}

		public override Commandlet Clone()
		{
			throw new NotImplementedException();
		}
	}*/
}