using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ProductionStepEvent : AbstractEvent {

		public int CurrentProduction { get; private set; }
		public int RequiredProduction { get; private set; }

		public ProductionStepEvent (EventAgent _source, int _currentProduction, int _requiredProduction) : base("productionStep", _source) {
			CurrentProduction = _currentProduction;
			RequiredProduction = _requiredProduction;
		}
	}
}