using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class GenericVectorCommandFactory : CommandFactory<Vector3>
    {
        public override string Name => "vector";
    }
}