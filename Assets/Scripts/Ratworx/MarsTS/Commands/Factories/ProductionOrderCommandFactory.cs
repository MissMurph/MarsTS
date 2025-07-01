using Ratworx.MarsTS.Production;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class ProductionOrderCommandFactory : CommandFactory<ProductionOption>
    {
        public override string Name => "produce";
    }
}