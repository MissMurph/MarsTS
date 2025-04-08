using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public interface IUnitInterface
    {
        GameObject GameObject { get; }
        Entity Entity { get; }
    }
}