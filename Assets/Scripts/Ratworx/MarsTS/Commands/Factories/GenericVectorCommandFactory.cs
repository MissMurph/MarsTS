using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class GenericVectorCommandFactory : CommandFactory<Vector3>
    {
        public override string Name => "vector";
        
        [Rpc(SendTo.Server)]
        protected override void ConstructCommandServerRpc(string commandKey, Vector3 target, int factionId, int[] selection, bool enqueue)
            => ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
    }
}