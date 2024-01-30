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

		protected override void FixedUpdate () {
			if (!deployed) base.FixedUpdate();
		}

		private void Deploy (Commandlet order) {
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

		protected override void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "attack":
				Attack(_event.Command);
				break;
				case "deploy":
				Deploy(_event.Command);
				break;
				default:
				base.ExecuteOrder(_event);
				break;
			}
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "deploy":
					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}
	}
}