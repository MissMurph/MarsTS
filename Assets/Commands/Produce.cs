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

	public class Produce : Command<GameObject> {

		public override string Name { get { return "produce/" + prefab.name; } }

		public override Sprite Icon { get { return unit.Icon; } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int timeRequired;

		[SerializeField]
		private CostEntry[] cost;

		private ISelectable unit;

		private void Awake () {
			unit = prefab.GetComponent<ISelectable>();
		}

		public override void StartSelection () {
			bool canAfford = true;

			foreach (CostEntry entry in cost) {
				if (Player.Main.Resource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (canAfford) {
				Player.Main.DeliverCommand(Construct(prefab), Player.Include);

				foreach (CostEntry entry in cost) {
					Player.Main.Resource(entry.key).Withdraw(entry.amount);
				}
			}
		}

		public override Commandlet Construct (GameObject _target) {
			return new ProductionCommandlet("produce", _target, Player.Main, timeRequired, prefab, unit, cost);
		}

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

	public class ProductionCommandlet : Commandlet<GameObject> {

		public int ProductionRequired { get; private set; }
		public int ProductionProgress { get; set; }
		public ISelectable Unit { get; private set; }
		public GameObject Prefab { get; private set; }
		public Dictionary<string, int> Cost { get; private set; }

		public ProductionCommandlet (string name, GameObject target, Faction commander, int timeRequired, GameObject prefab, ISelectable unit, CostEntry[] cost) : base(name, target, commander) {
			ProductionRequired = timeRequired;
			ProductionProgress = 0;
			Prefab = prefab;
			Unit = unit;

			Cost = new Dictionary<string, int>();

			foreach (CostEntry entry in cost) {
				Cost[entry.key] = entry.amount;
			}

			Callback.AddListener(OnCancel);
		}

		private void OnCancel (CommandCompleteEvent _event) {
			foreach (KeyValuePair<string, int> entry in Cost) {
				Player.Main.Resource(entry.Key).Deposit(entry.Value);
			}
		}
	}

	[Serializable]
	public class CostEntry {
		public string key;
		public int amount;
	}
}