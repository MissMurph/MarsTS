using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Unity.Netcode;

namespace Ratworx.MarsTS.Commands.Factories {
	/// <summary>
	/// Stub concrete class so this can be added as a network behaviour to a prefab
	/// </summary>
	public class GenericAttackableCommandFactory : CommandFactory<IAttackable> 
	{
		public override string Name => "attack";
		
		[Rpc(SendTo.Server)]
		protected override void ConstructCommandServerRpc(string commandKey, IAttackable target, int factionId, int[] selection, bool enqueue)
			=> ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
	}
}