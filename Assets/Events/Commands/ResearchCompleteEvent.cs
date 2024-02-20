using MarsTS.Commands;
using MarsTS.Research;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

    public class ResearchCompleteEvent : ProductionEvent {

        public Technology Tech { get; private set; }

        public ResearchCompleteEvent (EventAgent _source, Technology _product, ISelectable _producer, ProductionQueue _queue, IProducable _order) 
            : base("ResearchComplete", _source, _producer, _queue, _order) {
            Tech = _product;
        }
    }
}