using Ratworx.MarsTS.Teams;
using Unity.Netcode;

namespace Ratworx.MarsTS.Commands.Factories

{
    /// <summary>
    /// Stub concrete class so this can be added as a network behaviour to a prefab
    /// </summary>
    public class GenericBooleanCommandFactory : CommandFactory<bool>
    {
        public override string Name => "generic_boolean";
        
        [Rpc(SendTo.Server)]
        protected override void ConstructCommandServerRpc(string commandKey, bool target, int factionId, int[] selection, bool enqueue)
            => ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
    }
}