using UnityEngine;

namespace Ratworx.MarsTS.Buildings.Ghosts
{
    public class LandmineSelectionGhost : BuildingSelectionGhost
    {
        public override void InitializeGhost(Building buildingBeingConstructed)
        {
            AllRenderers = GetComponentsInChildren<Renderer>();

            ChangeAllRenderers(legalMat);
        }
    }
}