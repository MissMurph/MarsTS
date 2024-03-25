using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

		protected override void ConstructCommandletServer (GameObject _target, int _factionId, NetworkObjectReference[] _selection, bool _inclusive) {
			ProductionCommandlet order = Instantiate(orderPrefab) as ProductionCommandlet;

			order.Init(Name, _target, TeamCache.Faction(_factionId), timeRequired, cost);

			foreach (NetworkObjectReference objectRef in _selection) {
				if (EntityCache.TryGet(((GameObject)objectRef).name, out ICommandable unit)) {
					unit.Order(order, _inclusive);
				}
			}
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

	public class ProductionCommandlet : Commandlet<GameObject>, IProducable {

		public int ProductionRequired { get; private set; }
		public int ProductionProgress { get => _productionProgress.Value; 
			set {
				int oldProgress = _productionProgress.Value;
				_productionProgress.Value = value;
				OnWork.Invoke(oldProgress, _productionProgress.Value);
			}
		}

		protected NetworkVariable<int> _productionProgress = new(writePerm:NetworkVariableWritePermission.Server);

		public event Action<int, int> OnWork;

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

		public void Init (string _name, GameObject _target, Faction _commander, int timeRequired, CostEntry[] cost) {
			Init(_name, _target, _commander);
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
		public event Action<int, int> OnWork;
	}
}