using Ratworx.MarsTS.WorldObject;

namespace Ratworx.MarsTS.Commands.Factories
{
    public class GenericHarvestableCommandFactory : CommandFactory<IHarvestable>
    {
        public override string Name => "harvest";
    }
}