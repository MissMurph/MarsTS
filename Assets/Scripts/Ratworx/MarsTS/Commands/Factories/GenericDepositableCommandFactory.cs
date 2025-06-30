using Ratworx.MarsTS.Buildings;

namespace Ratworx.MarsTS.Commands.Factories
{
    /// <summary>
    /// Stub concrete class so this can be added as a network behaviour to a prefab
    /// </summary>
    public class GenericDepositableCommandFactory : CommandFactory<IDepositable>
    {
        public override string Name => "deposit";
    }
}