using MarsTS.Units;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public interface ICommandable {
		GameObject GameObject { get; }
		Commandlet CurrentCommand { get; }
		Commandlet[] CommandQueue { get; }
		List<string> Active { get; }
		List<Cooldown> Cooldowns { get; }
		void Order (Commandlet order, bool inclusive);
		Command Evaluate (ISelectable target);
		Commandlet Auto (ISelectable target);
		string[] Commands ();
		bool CanCommand (string key);
	}
}