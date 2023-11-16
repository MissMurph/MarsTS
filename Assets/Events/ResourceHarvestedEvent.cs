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
		//public ISelectable Harvester { get; private set; }
		
		public ResourceHarvestedEvent (EventAgent _source, IHarvestable _deposit, int _harvestAmount, string _resourceType) : base("resourceHarvested", _source) {
			Deposit = _deposit;
			HarvestAmount = _harvestAmount;
			ResourceType = _resourceType;
			//Harvester = _harvester;
		}
	}
}