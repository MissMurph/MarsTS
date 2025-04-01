using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Harvesting {

	public class HarvesterDepositEvent : AbstractEvent {

		public ISelectable Harvester { get; private set; }
		public int StoredAmount { get; private set; }
		public int Capacity { get; private set; }
		public IDepositable Bank { get; private set; }
		public Side EventSide { get; private set; }

		public HarvesterDepositEvent (EventAgent _source, ISelectable _harvester, Side _eventSide, int _storedAmount, int _capacity, IDepositable _bank) : base("harvesterDeposit", _source) {
			Harvester = _harvester;
			StoredAmount = _storedAmount;
			Capacity = _capacity;
			Bank = _bank;
			EventSide = _eventSide;
		}

		public enum Side {
			Bank,
			Harvester
		}
	}
}