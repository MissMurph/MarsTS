using UnityEngine;

namespace MarsTS.Buildings
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