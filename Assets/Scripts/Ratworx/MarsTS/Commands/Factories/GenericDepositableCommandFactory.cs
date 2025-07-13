using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;

namespace Ratworx.MarsTS.Commands.Factories
{
    /// <summary>
    /// Stub concrete class so this can be added as a network behaviour to a prefab
    /// </summary>
    public class GenericDepositableCommandFactory : CommandFactory<IDepositable>
    {
        public override string Name => "deposit";
        
        [Rpc(SendTo.Server)]
        protected override void ConstructCommandServerRpc(string commandKey, IDepositable target, int factionId, int[] selection, bool enqueue)
            => ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
    }
}