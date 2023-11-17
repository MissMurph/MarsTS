using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.World {

    public interface IHarvestable {
		GameObject GameObject { get; }
		int OriginalAmount { get; }
		int StoredAmount { get; }
		int Harvest (string resourceKey, int harvestAmount, Func<int, int> extractor);
		bool CanHarvest (string resourceKey, ISelectable unit);
	}
}