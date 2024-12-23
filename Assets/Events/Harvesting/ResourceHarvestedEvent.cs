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
            EventAgent source, 
            ISelectable unit, 
            Side eventSide, 
            int harvestAmount, 
            string resourceType, 
            int storedAmount, 
            int capacity
        ) : base(
			$"{resourceType}Harvested", 
			source, 
			resourceType
		) {
			Unit = unit;
			HarvestAmount = harvestAmount;
			EventSide = eventSide;
			StoredAmount = storedAmount;
			Capacity = capacity;
		}

		public enum Side {
			Deposit,
			Harvester
		}
	}
}