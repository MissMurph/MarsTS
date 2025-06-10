using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Production;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class ProductionBar : UnitBar
    {
        private ProductionQueue _productionQueue;

        private void Awake() {
            _productionQueue = GetComponentInParent<ProductionQueue>();
        }

        private void Start() {
            _barRenderer.enabled = false;

            EventAgent bus = GetComponentInParent<EventAgent>();

            bus.AddListener<ProductionStepEvent>(evnt =>
            {
                if (evnt.Name != "productionStep") return;
                if (!_barRenderer.enabled) _barRenderer.enabled = true;
                UpdateBarWithFillLevel(evnt.PercentageProgress);
            });

            _productionQueue.OnOrderComplete += () =>
            {
                UpdateBarWithFillLevel(0f);
                _barRenderer.enabled = false;
            };
        }
    }
}