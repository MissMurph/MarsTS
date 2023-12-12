using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class ProductionBar : UnitBar {

		private void Start () {
			barRenderer.enabled = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			bus.AddListener<ProductionEvent>((_event) => {
				if (_event.Name != "productionStep") return;
				if (!barRenderer.enabled) barRenderer.enabled = true; 
				FillLevel = (float)_event.CurrentProduction.ProductionProgress / _event.CurrentProduction.ProductionRequired;
			});

			bus.AddListener<UnitProducedEvent>((_event) => {
				barRenderer.enabled = false;
				FillLevel = 0f;
			});
		}
	}
}