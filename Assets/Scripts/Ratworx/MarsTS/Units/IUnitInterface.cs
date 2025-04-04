using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public interface IUnitInterface
    {
        GameObject GameObject { get; }
        IUnitInterface UnitInterface { get; }
    }
}