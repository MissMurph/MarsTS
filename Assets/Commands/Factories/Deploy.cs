using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Entities;
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
		private float deployTime;

		public override void StartSelection () {
			int totalCanDeploy = 0;
			int totalDeployed = 0;

			//Inspect all selected to make all units using this ability match up with others that are active using
			foreach (Roster rollup in Player.Selected.Values) {
				if (rollup.Commands.Contains(Name)) {
					totalCanDeploy += rollup.Count;

					foreach (ICommandable unit in rollup.Orderable) {
						if (unit.Active.Count == 0) continue;

						foreach (string activeCommand in unit.Active) {
							if (activeCommand == Name) totalDeployed++;
						}
					}
				}
			}

			//Player.Main.DeliverCommand(Construct(totalCanDeploy > totalDeployed), Player.Include);

			//Player.Main.DeliverCommand(new Commandlet<bool>(Name, true, Player.Main), Player.Include);
		}

		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = 5 } };
		}

		public override void CancelSelection () {

		}

		/*public override Commandlet Construct (bool _target) {
			return new DeployCommandlet(Name, _target, deployTime);
		}*/
	}

	public class DeployCommandlet : Commandlet<bool>, IWorkable {

		public float WorkRequired { get; private set; }
		public float CurrentWork { get; set; }

		public override string Key => Name;

		public DeployCommandlet (string _name, bool _status, float _workRequired) {
			WorkRequired = _workRequired;
			CurrentWork = 0f;
		}

		public override void StartCommand (EventAgent eventAgent, ICommandable unit) {
			base.StartCommand(eventAgent, unit);

			if (EntityCache.TryGet($"{unit.GameObject.name}:commandQueue", out CommandQueue queue))
				queue.Cooldown(this, WorkRequired);
		}

		public override void CompleteCommand (EventAgent eventAgent, ICommandable unit, bool isCancelled = false) {
			base.CompleteCommand(eventAgent, unit, isCancelled);

			if (EntityCache.TryGet($"{unit.GameObject.name}:commandQueue", out CommandQueue queue))
				queue.Activate(this, Target);
		}

		public override bool CanInterrupt () {
			return false;
		}

		public override Commandlet Clone()
		{
			throw new NotImplementedException();
		}
	}

	public interface IWorkable {
		float WorkRequired { get; }
		float CurrentWork { get; set; }
	}
}