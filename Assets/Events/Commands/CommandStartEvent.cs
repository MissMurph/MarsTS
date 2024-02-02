using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	//Stinky
	public class CommandStartEvent : CommandEvent {
		public CommandStartEvent (EventAgent _source, Commandlet _command, ISelectable _unit) : base("Started", _source, _command, _unit) {
		}
	}
}