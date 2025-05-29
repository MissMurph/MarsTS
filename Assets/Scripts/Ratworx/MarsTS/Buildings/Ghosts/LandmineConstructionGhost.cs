using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Production;
using Unity.Netcode;

namespace Ratworx.MarsTS.Buildings.Ghosts
{
    public class LandmineConstructionGhost : BuildingConstructionGhost
    {
        public override void InitializeGhost(string buildingBeingConstructed, params ResourceCost[] constructionCost) {
            if (!NetworkManager.Singleton.IsServer) return;

            UpdateProperties(buildingBeingConstructed, constructionCost);
        }
    }
}