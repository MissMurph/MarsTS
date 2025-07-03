using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Production;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public interface ICostingCommand
    {
        public ResourceCost[] GetCost(string argument = null);
    }
}