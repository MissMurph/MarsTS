using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public class Research : CommandFactory<GameObject> {

		public override string Name { get { return "research/" + prefab.name; } }

		public override string Description { get { return description; } }

		[SerializeField]
		protected string description;

		[SerializeField]
		protected GameObject prefab;

		[SerializeField]
		protected int timeRequired;

		[SerializeField]
		protected CostEntry[] cost;

		public override void StartSelection () {
			bool canAfford = true;

			foreach (CostEntry entry in cost) {
				if (Player.Commander.Resource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (canAfford) {
				//Player.Main.DistributeCommand(Construct(prefab), Player.Include);

				foreach (CostEntry entry in cost) {
					Player.Commander.Resource(entry.key).Withdraw(entry.amount);
				}
			}
		}

		/*public override Commandlet Construct (GameObject _target) {
			return new UpgradeCommandlet("research", _target, timeRequired, cost);
		}*/

		public override CostEntry[] GetCost () {
			List<CostEntry> spool = new List<CostEntry>();

			foreach (CostEntry entry in cost) {
				spool.Add(entry);
			}

			CostEntry time = new CostEntry();
			time.key = "time";
			time.amount = timeRequired;

			spool.Add(time);

			return spool.ToArray();
		}

		public override void CancelSelection () {

		}
	}
}