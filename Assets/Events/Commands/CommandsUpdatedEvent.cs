using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CommandsUpdatedEvent : AbstractEvent {

		public string[] NewCommands { get; private set; }
		public ISelectable Unit { get; private set; }

		public CommandsUpdatedEvent (EventAgent _source, ISelectable _unit, params string[] _newCommands) : base("commandsUpdated", _source) {
			NewCommands = _newCommands;
			Unit = _unit;
		}
	}
}