using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.World {

    public interface IHarvestable {
		GameObject GameObject { get; }
		int Harvest (string resourceKey, int amount);
		bool CanHarvest (string resourceKey, ISelectable unit);
	}
}