using System;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.WorldObject {

    public interface IHarvestable : IUnitInterface {
		int OriginalAmount { get; }
		int StoredAmount { get; }
		int Harvest (string resourceKey, ISelectable harvester, int harvestAmount, Func<int, int> extractor);
		bool CanHarvest (string resourceKey, ISelectable unit);
	}
}