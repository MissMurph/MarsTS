using System.Linq;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Teams;

namespace Ratworx.MarsTS.Extensions
{
    public static class ConstructionOptionExtensions
    {
        public static bool CanFactionAfford(this ConstructionOption option, Faction faction)
            => !option.Cost.Any(entry => faction.GetResource(entry.key).Amount < entry.amount);
    }
}