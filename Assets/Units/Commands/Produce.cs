using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

	public class Produce : Command<GameObject> {

		public override string Name { get { return "produce/" + prefab.name; } }

		[SerializeField]
		private GameObject prefab;

		[SerializeField]
		private int timeRequired;

		public override void StartSelection () {
			Player.Main.DeliverCommand(Construct(prefab), Player.Include);
		}

		public override Commandlet Construct (GameObject _target) {
			return new ProductionCommandlet("produce", _target, Player.Main, timeRequired);
		}
	}

	public class ProductionCommandlet : Commandlet<GameObject> {

		public int TimeRequired { get; private set; }
		public int ProductionProgress { get; private set; }

		public ProductionCommandlet (string name, GameObject target, Faction commander, int timeRequired) : base(name, target, commander) {
			TimeRequired = timeRequired;
			ProductionProgress = 0;
		}
	}
}