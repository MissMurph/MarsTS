using MarsTS.Commands;
using Unity.Netcode;

namespace MarsTS.Buildings
{
    public class LandmineConstructionGhost : BuildingConstructionGhost
    {
        public override void InitializeGhost(Building buildingBeingConstructed, int constructionWorkRequired,
            params CostEntry[] constructionCost)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            UpdateProperties(buildingBeingConstructed, constructionWorkRequired, constructionCost);
        }
    }
}