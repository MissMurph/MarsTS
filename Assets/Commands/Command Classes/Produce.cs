using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

	public class Produce : CommandFactory<GameObject> {

		public override string Name { get { return "produce/" + prefab.name; } }

		public override Sprite Icon { get { return unit.Icon; } }

		public override string Description { get { return description; } }

		[SerializeField]
		protected string description;

		[SerializeField]
		protected GameObject prefab;

		[SerializeField]
		protected int timeRequired;

		[SerializeField]
		protected CostEntry[] cost;

		private ISelectable unit;

		private void Awake () {
			unit = prefab.GetComponent<ISelectable>();
		}

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
			return new ProductionCommandlet("produce", _target, timeRequired, cost);
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

	public class ProductionCommandlet : Commandlet<GameObject>, IProducable {

		public int ProductionRequired { get; private set; }
		public int ProductionProgress { get; set; }
		public Dictionary<string, int> Cost { get; private set; }
		public GameObject Product { get { return Target; } }
		public override CommandFactory Command { get { return CommandRegistry.Get(Name + "/" + Product.name); } }

		public ProductionCommandlet (string name, GameObject prefab, int timeRequired, CostEntry[] cost) {
			ProductionRequired = timeRequired;
			ProductionProgress = 0;

			Cost = new Dictionary<string, int>();

			foreach (CostEntry entry in cost) {
				Cost[entry.key] = entry.amount;
			}
		}

		public Commandlet Get () {
			return this;
		}

		public override void OnComplete (CommandQueue queue, CommandCompleteEvent _event) {
			if (_event.CommandCancelled) {
				foreach (KeyValuePair<string, int> entry in Cost) {
					Player.Commander.Resource(entry.Key).Deposit(entry.Value);
				}
			}

			base.OnComplete (queue, _event);
		}
	}

	[Serializable]
	public class CostEntry {
		public string key;
		public int amount;
	}

	public interface IProducable {
		public int ProductionRequired { get; }
		public int ProductionProgress { get; set; }
		public Dictionary<string, int> Cost { get; }
		public GameObject Product { get; }
		Commandlet Get ();
	}
}