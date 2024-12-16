using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ResourceHarvestedEvent : ResourceEvent {
		
		public ISelectable Unit { get; private set; }
		public Side EventSide { get; private set; }

		public int HarvestAmount { get; private set; }
		public int StoredAmount { get; private set; }
		public int Capacity { get; private set; }


		public ResourceHarvestedEvent (
            EventAgent _source, 
            ISelectable unit, 
            Side _eventSide, 
            int _harvestAmount, 
            string _resourceType, 
            int _storedAmount, 
            int _capacity
        ) : base(
			"Harvested", 
			_source, 
			_resourceType
		) {
			Unit = unit;
			HarvestAmount = _harvestAmount;
			EventSide = _eventSide;
			StoredAmount = _storedAmount;
			Capacity = _capacity;
		}

		public enum Side {
			Deposit,
			Harvester
		}
	}
}