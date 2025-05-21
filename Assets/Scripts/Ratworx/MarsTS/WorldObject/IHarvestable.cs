using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.WorldObject
{
    public interface IHarvestable : IUnitInterface
    {
        int OriginalAmount { get; }
        int StoredAmount { get; }
        string Resource { get; }
        int Harvest(string resourceKey, Entity harvester, int harvestAmount, Func<int, int> extractor);
        bool CanHarvest(string resourceKey, Entity unit);
    }
}