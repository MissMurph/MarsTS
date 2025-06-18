using System;
using Ratworx.MarsTS.Entities;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public interface IUnitInterface : IEquatable<Entity>
    {
        GameObject GameObject { get; }
        Entity Entity { get; }
        bool IEquatable<Entity>.Equals(Entity other) => Entity.Equals(other);
    }
}