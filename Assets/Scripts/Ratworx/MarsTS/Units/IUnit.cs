using UnityEngine;

namespace MarsTS.Units
{
    public interface IUnit
    {
        GameObject GameObject { get; }
        IUnit Unit { get; }
    }
}