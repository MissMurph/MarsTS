using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class ProductionBar : UnitBar {

		private void Start () {
			barRenderer.enabled = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			bus.AddListener<ProductionStepEvent>((_event) => {
				if (!barRenderer.enabled) barRenderer.enabled = true; 
				FillLevel = (float)_event.CurrentProduction / _event.RequiredProduction;
			});

			bus.AddListener<ProductionEvent>((_event) => {
				barRenderer.enabled = false;
				FillLevel = 0f;
			});
		}
	}
}