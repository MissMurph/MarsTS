using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Production;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class ProductionBar : UnitBar
    {
        private ProductionQueue _productionQueue;

        protected override void Awake() {
            base.Awake();
            
            _productionQueue = GetComponentInParent<ProductionQueue>();
        }

        private void Start() {
            BarRenderer.enabled = false;

            EventAgent bus = GetComponentInParent<EventAgent>();

            bus.AddListener<ProductionStepEvent>(evnt =>
            {
                if (evnt.Name != "productionStep") return;
                if (!BarRenderer.enabled) BarRenderer.enabled = true;
                UpdateBarWithFillLevel(evnt.PercentageProgress);
            });

            _productionQueue.OnOrderComplete += () =>
            {
                UpdateBarWithFillLevel(0f);
                BarRenderer.enabled = false;
            };
        }
    }
}