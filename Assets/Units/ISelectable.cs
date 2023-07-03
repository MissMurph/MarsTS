using MarsTS.Units.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {
	public interface ISelectable {
		void Enqueue (Commandlet order);
		void Execute (Commandlet order);
		Unit Get ();
		string[] Commands ();
		void Select (bool status);
		int Id ();
		string Type ();
	}
}