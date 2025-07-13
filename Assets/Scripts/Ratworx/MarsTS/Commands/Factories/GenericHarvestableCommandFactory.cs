using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.WorldObject;
using Unity.Netcode;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class GenericHarvestableCommandFactory : CommandFactory<IHarvestable>
    {
        public override string Name => "harvest";
        
        [Rpc(SendTo.Server)]
        protected override void ConstructCommandServerRpc(string commandKey, IHarvestable target, int factionId, int[] selection, bool enqueue)
            => ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
    }
}