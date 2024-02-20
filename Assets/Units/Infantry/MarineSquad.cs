using MarsTS.Commands;
using MarsTS.Players;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class MarineSquad : InfantrySquad {

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "attack":

					break;
				case "adrenaline":
					SquadBoost(order);
					return;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}

		private void SquadBoost (Commandlet order) {
			if (!CanCommand(order.Name)) return;
			Commandlet<bool> deserialized = order as Commandlet<bool>;

			commands.Activate(order, deserialized.Target);

			foreach (InfantryMember unit in members) {
				unit.Order(order, false);
			}
		}

		public override Command Evaluate (ISelectable target) {
			if (target is IAttackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get("attack");
			}

			return CommandRegistry.Get("move");
		}

		public override Commandlet Auto (ISelectable target) {
			if (target is IAttackable attackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get<Attack>("attack").Construct(attackable);
			}

			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		public override bool CanCommand (string key) {
			if (key == "adrenaline") {
				return owner.IsResearched("research/adrenaline");
			}

			return base.CanCommand(key);
		}
	}
}