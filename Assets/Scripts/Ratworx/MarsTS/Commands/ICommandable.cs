using System.Collections.Generic;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Commands {

    public interface ICommandable : IUnitInterface {
		Commandlet CurrentCommand { get; }
		int Count { get; }
		List<string> Active { get; }
		List<Timer> Cooldowns { get; }
		void Order (Commandlet order, bool inclusive);
		CommandFactory Evaluate (ISelectable target);
		void AutoCommand (ISelectable target);
		string[] Commands ();
		bool CanCommand (string key);
	}
}