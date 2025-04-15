using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public abstract class AbstractLocomotion : MonoBehaviour,
                                      IEntityComponent<AbstractLocomotion>,
                                      IEntityPhysicsUpdate,
                                      IUnitInterface
    {
        
    }
}