using MarsTS.Units.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class Factory : Building {

		//How many steps will occur per second
		[SerializeField]
		private float productionSpeed;

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "produce":
					Produce(order);
					break;
				default:
					base.ProcessOrder(order);
					break;
			}
		}

		protected virtual void Produce (Commandlet order) {

		}
	}
}