using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ProductionStepEvent : AbstractEvent {

		public int CurrentProduction { get; private set; }
		public int RequiredProduction { get; private set; }
		public ISelectable Unit { get; private set; }

		public ProductionStepEvent (EventAgent _source, ProductionCommandlet _order) : base("productionStep", _source) {
			CurrentProduction = _order.ProductionProgress;
			RequiredProduction = _order.ProductionRequired;
			Unit = _order.Unit;
		}
	}
}