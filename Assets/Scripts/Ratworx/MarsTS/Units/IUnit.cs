using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public interface IUnit
    {
        GameObject GameObject { get; }
        IUnit Unit { get; }
    }
}