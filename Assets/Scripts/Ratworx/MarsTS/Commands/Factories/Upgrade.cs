using MarsTS.Events;
using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

	public class Upgrade : Produce {
		public override string Name { get { return "upgrade/" + _unitPrefab.name; } }

		public override Sprite Icon { get { return icon; } }

		public override string Description { get { return _description; } }

		public override void StartSelection () {
			bool canAfford = true;

			foreach (CostEntry entry in _cost) {
				if (Player.Commander.GetResource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (canAfford) {
				//Player.Main.DeliverCommand(Construct(prefab), Player.Include);

				foreach (CostEntry entry in _cost) {
					Player.Commander.GetResource(entry.key).Withdraw(entry.amount);
				}
			}
		}

		/*public override Commandlet Construct (GameObject _target) {
			return new UpgradeCommandlet("upgrade", _target, timeRequired, cost);
		}*/

		public override CostEntry[] GetCost () {
			List<CostEntry> spool = new List<CostEntry>();

			foreach (CostEntry entry in _cost) {
				spool.Add(entry);
			}

			CostEntry time = new CostEntry();
			time.key = "time";
			time.amount = _timeRequired;

			spool.Add(time);

			return spool.ToArray();
		}

		public override void CancelSelection () {

		}
	}
}