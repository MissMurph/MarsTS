using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Extensions
{
    public static class RosterExtensions
    {
        public static bool TryGetCommandables(this Roster roster, out List<ICommandable> output) {
            output = new List<ICommandable>();

            foreach (Entity entity in roster) {
                // If this unit cannot be commanded, then none of them will have the interface, thus we check only once
                if (!entity.TryGetEntityComponent(out ICommandable commandable)) return false;

                output.Add(commandable);
            }

            return true;
        }

        public static List<ICommandable> GetCommandables(this Roster roster) {
            var output = new List<ICommandable>();

            foreach (Entity entity in roster) {
                // If this unit cannot be commanded, then none of them will have the interface, thus we check only once
                if (!entity.TryGetEntityComponent(out ICommandable commandable)) break;

                output.Add(commandable);
            }

            return output;
        }
        
        public static List<string> GetCommands(this Roster roster) {
            var output = new List<string>();

            if (roster.GetFirst().TryGetEntityComponent(out ICommandable commandable)) 
                output.AddRange(commandable.Commands().Select(kvp => kvp.Key));

            return output;
        }

        public static bool IsCommandable(this Roster roster) 
            => roster.GetFirst().TryGetEntityComponent(out ICommandable _);
    }
}