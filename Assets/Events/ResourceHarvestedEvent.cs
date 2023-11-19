using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class ResourceHarvestedEvent : AbstractEvent {
		
		public IHarvestable Deposit { get; private set; }
		public int HarvestAmount { get; private set; }
		public string ResourceType { get; private set; }
		
		
		public ResourceHarvestedEvent (EventAgent _source, IHarvestable _deposit, int _harvestAmount, string _resourceType) : base("resourceHarvested", _source) {
			Deposit = _deposit;
			HarvestAmount = _harvestAmount;
			ResourceType = _resourceType;
		}
	}

	public class HarvesterExtractionEvent : AbstractEvent {

		public ISelectable Harvester { get; private set; }
		public int StoredAmount { get; private set; }
		public int Capacity { get; private set; }
		public IHarvestable Deposit { get; private set; }

		public HarvesterExtractionEvent (EventAgent _source, ISelectable _harvester, int _storedAmount, int _capacity, IHarvestable _deposit) : base("harvesterExtraction", _source) {
			Harvester = _harvester;
			StoredAmount = _storedAmount;
			Capacity = _capacity;
			Deposit = _deposit;
		}
	}
}