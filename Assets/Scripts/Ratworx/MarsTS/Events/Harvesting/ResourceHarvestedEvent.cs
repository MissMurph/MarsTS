using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Harvesting {

	public class ResourceHarvestedEvent : ResourceEvent {
		
		public Entity Unit { get; private set; }
		public Side EventSide { get; private set; }
		public int HarvestAmount { get; private set; }
		public int StoredAmount { get; private set; }
		public int Capacity { get; private set; }

		public ResourceHarvestedEvent (
            Entity unit, 
            Side eventSide, 
            int harvestAmount, 
            string resourceType, 
            int storedAmount, 
            int capacity
        ) : base(
			$"{resourceType}Harvested", 
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