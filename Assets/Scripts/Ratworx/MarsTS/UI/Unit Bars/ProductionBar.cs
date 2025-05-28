using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;

namespace Ratworx.MarsTS.UI.Unit_Bars {

    public class ProductionBar : UnitBar {

		private void Start () {
			_barRenderer.enabled = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			bus.AddListener<ProductionStepEvent>((_event) => {
				if (_event.Name != "productionStep") return;
				if (!_barRenderer.enabled) _barRenderer.enabled = true; 
				UpdateBarWithFillLevel((float)_event.CurrentProduction.ProductionProgress / _event.CurrentProduction.ProductionRequired);
			});

			bus.AddListener<ProductionStepCompleteEvent>((_event) => {
				_barRenderer.enabled = false;
				UpdateBarWithFillLevel(0f);
			});
		}
	}
}