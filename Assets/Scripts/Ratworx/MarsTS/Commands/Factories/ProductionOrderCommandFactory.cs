using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class ProductionOrderCommandFactory : CommandFactory<ProductionOption>
    {
        public override string Name => "produce";
        
        [Rpc(SendTo.Server)]
        protected override void ConstructCommandServerRpc(string commandKey, ProductionOption target, int factionId, int[] selection, bool enqueue)
            => ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
    }
}