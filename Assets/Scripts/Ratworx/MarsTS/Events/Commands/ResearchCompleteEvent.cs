using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Research;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Commands {

    public class ResearchCompleteEvent : ProductionEvent {

        public Technology Tech { get; private set; }

        public ResearchCompleteEvent (EventAgent _source, Technology _product, ISelectable _producer, ProductionQueue _queue, IProducable _order) 
            : base("ResearchComplete", _source, _producer, _queue, _order) {
            Tech = _product;
        }
    }
}