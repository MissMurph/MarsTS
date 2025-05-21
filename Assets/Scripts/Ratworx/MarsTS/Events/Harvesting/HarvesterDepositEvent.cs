using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Harvesting {

	public class HarvesterDepositEvent : AbstractEvent {

		public Entity Harvester { get; private set; }
		public int StoredAmount { get; private set; }
		public int Capacity { get; private set; }
		public IDepositable Bank { get; private set; }
		public Side EventSide { get; private set; }

		public HarvesterDepositEvent (
            Entity harvester,
            Side eventSide,
            int storedAmount,
            int capacity,
            IDepositable bank
		) : base(
			"harvesterDeposit"
		) {
			Harvester = harvester;
			StoredAmount = storedAmount;
			Capacity = capacity;
			Bank = bank;
			EventSide = eventSide;
		}

		public enum Side {
			Bank,
			Harvester
		}
	}
}