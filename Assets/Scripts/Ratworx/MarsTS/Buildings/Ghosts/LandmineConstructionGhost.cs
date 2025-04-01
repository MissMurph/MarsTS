using Ratworx.MarsTS.Commands.Factories;
using Unity.Netcode;

namespace Ratworx.MarsTS.Buildings.Ghosts
{
    public class LandmineConstructionGhost : BuildingConstructionGhost
    {
        public override void InitializeGhost(string buildingBeingConstructed, int constructionWorkRequired,
            params CostEntry[] constructionCost)
        {
            if (!NetworkManager.Singleton.IsServer) return;

            UpdateProperties(buildingBeingConstructed, constructionWorkRequired, constructionCost);
        }
    }
}