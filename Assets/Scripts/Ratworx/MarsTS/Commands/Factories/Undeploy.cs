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

			foreach (Roster rollup in Player.Player.Selected.Values) {
				if (!rollup.GetCommands().Contains(Name)) 
					continue;
				
				foreach (ICommandable unit in rollup.GetCommandables()) {
					if (unit.ActiveCommands.Count == 0) continue;
					if (unit.Commands()[Name].IsActive) continue;

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
		private void ConstructCommandletServerRpc(int factionId, int[] selection, bool inclusive) {
			ConstructCommandServer(false, factionId, selection, inclusive);
		}

		public override ResourceCost[] GetCost () {
			return new ResourceCost[1] { new ResourceCost { key = "time", amount = 5 } };
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