using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Production;

namespace Ratworx.MarsTS.Events.Commands
{
    public class ProductionStepEvent : UnitEvent
    {
        public ProductionOrder ProductionOrder { get; private set; }
        public float PercentageProgress { get; private set; }

        public ProductionStepEvent(
            Entity producer,
            ProductionOrder order,
            float percentageProgress
        ) : base(
            "productionStep",
            producer
        ) {
            ProductionOrder = order;
            PercentageProgress = percentageProgress;
        }
    }
}