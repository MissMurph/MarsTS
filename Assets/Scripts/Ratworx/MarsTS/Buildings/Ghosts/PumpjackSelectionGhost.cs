using MarsTS.Entities;
using MarsTS.World;
using UnityEngine;

namespace MarsTS.Buildings
{
    public class PumpjackSelectionGhost : BuildingSelectionGhost
    {
        public override bool Legal
        {
            get
            {
                bool valid = false;

                foreach (Collider other in Collisions)
                {
                    if (EntityCache.TryGet(other.transform.root.name, out ResourceDeposit comp) && comp is OilDeposit)
                        valid = true;
                    else
                        valid = false;
                }

                return valid;
            }
        }
    }
}