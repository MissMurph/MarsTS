using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class MobileArtillery : Tank {

		private bool deployed = false;

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "attack":
					CurrentCommand = order;
					Attack(order);
					break;
				case "deploy":
					Deploy();
					break;
				default:
					base.ProcessOrder(order);
					break;
			}
		}

		protected override void FixedUpdate () {
			if (!deployed) base.FixedUpdate();
		}

		private void Deploy () {
			if (deployed) {
				currentTopSpeed = topSpeed;
				deployed = false;
			}
			else {
				currentTopSpeed = 0f;
				body.velocity = Vector3.zero;
				deployed = true;
			}

			bus.Local(new DeployEvent(bus, this, deployed));
		}

		public override void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			if (order.Name == "deploy") {
				Deploy();
				return;
			}

			commandQueue.Clear();

			currentPath = Path.Empty;
			TrackedTarget = null;

			if (CurrentCommand != null) {
				CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, true, this);
				CurrentCommand.Callback.Invoke(_event);
				bus.Global(_event);
			}

			CurrentCommand = null;
			commandQueue.Enqueue(order);
		}
	}
}