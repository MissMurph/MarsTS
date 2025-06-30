using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Commands.Factories {
	/// <summary>
	/// Stub concrete class so this can be added as a network behaviour to a prefab
	/// </summary>
	public class GenericAttackableCommandFactory : CommandFactory<IAttackable> {
		public override string Name => "attack";
	}
}