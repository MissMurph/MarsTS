using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    public interface ICommandable {
		GameObject GameObject { get; }
		Commandlet CurrentCommand { get; }
		void Enqueue (Commandlet order);
		void Execute (Commandlet order);
		Command Evaluate (ISelectable target);
		Commandlet Auto (ISelectable target);
		string[] Commands ();
	}
}