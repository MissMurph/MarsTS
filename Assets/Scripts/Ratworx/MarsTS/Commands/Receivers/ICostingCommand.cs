using Ratworx.MarsTS.Commands.Factories;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public interface ICostingCommand
    {
        public CostEntry[] GetCost();
    }
}