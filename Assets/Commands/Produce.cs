using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

	public class Produce : Command<GameObject> {

		public override string Name { get { return "produce/" + prefab.name; } }

		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int timeRequired;

		private ISelectable unit;

		private void Awake () {
			unit = prefab.GetComponent<ISelectable>();
		}

		public override void StartSelection () {
			Player.Main.DeliverCommand(Construct(prefab), Player.Include);
		}

		public override Commandlet Construct (GameObject _target) {
			return new ProductionCommandlet("produce", _target, Player.Main, timeRequired, prefab, unit);
		}
	}

	public class ProductionCommandlet : Commandlet<GameObject> {

		public int ProductionRequired { get; private set; }
		public int ProductionProgress { get; set; }
		public ISelectable Unit { get; private set; }
		public GameObject Prefab { get; private set; }

		public ProductionCommandlet (string name, GameObject target, Faction commander, int timeRequired, GameObject prefab, ISelectable unit) : base(name, target, commander) {
			ProductionRequired = timeRequired;
			ProductionProgress = 0;
			Prefab = prefab;
			Unit = unit;
		}
	}
}