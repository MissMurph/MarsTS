using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;

namespace Ratworx.MarsTS.Extensions
{
    public static class ProductionOptionExtensions
    {
        public static bool CanFactionAfford(this ProductionOption option, Faction faction)
            => !option.Cost.Any(entry => faction.GetResource(entry.key).Amount < entry.amount);

        public static bool CanFactionAfford(this ResourceCost[] costs, Faction faction)
            => !costs.Where(entry => entry.key != "time")
                .Any(entry => faction.GetResource(entry.key).Amount < entry.amount);
    }
}