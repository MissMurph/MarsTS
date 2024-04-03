using MarsTS.Commands;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MarsTS.Units {

	public class MobileArtillery : Tank {

		[SerializeField]
		private bool deployed;

		private int deployCommandIndex;

		protected override void Awake () {
			base.Awake();

			for (int i = 0; i < boundCommands.Length; i++) {
				if (boundCommands[i] == "deploy") {
					deployCommandIndex = i;
					break;
				}
			}
		}

		protected void Start () {

			if (deployed) bus.Local(new DeployEvent(bus, this, deployed));
		}

		protected override void FixedUpdate () {
			if (!deployed) base.FixedUpdate();
		}

		private void Deploy (Commandlet order) {
			if ((order as Commandlet<bool>).Target) {
				currentTopSpeed = 0f;
				body.velocity = Vector3.zero;
				deployed = true;
			}
			else {
				bus.Local(new DeployEvent(bus, this, false));
			}

			bus.AddListener<CommandCompleteEvent>(DeployComplete);
		}

		private void DeployComplete (CommandCompleteEvent _event) {
			bus.RemoveListener<CommandCompleteEvent>(DeployComplete);

			deployed = (_event.Command as Commandlet<bool>).Target;

			if (deployed) {
				boundCommands[deployCommandIndex] = "undeploy";
				bus.Local(new DeployEvent(bus, this, deployed));
			}
			else {
				currentTopSpeed = topSpeed;
				boundCommands[deployCommandIndex] = "deploy";
			}
			
			bus.Global(new CommandsUpdatedEvent(bus, this, boundCommands));
		}

		protected override void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "attack":
					Attack(_event.Command);
					break;
				case "deploy":
					Deploy(_event.Command);
					break;
				case "undeploy":
					Deploy(_event.Command);
					break;
				default:
					base.ExecuteOrder(_event);
					break;
			}
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "deploy":
					if ((order as Commandlet<bool>).Target == deployed) return;
					break;
				case "undeploy":
					if ((order as Commandlet<bool>).Target == deployed) return;
					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}

		public override bool CanCommand (string key) {
			if (key == "move" && deployed) return false;

			return base.CanCommand(key);
		}
	}
}