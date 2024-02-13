using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CommandEvent : AbstractEvent {

		public Commandlet Command { get; private set; }
		public ISelectable Unit { get; private set; }

		public CommandEvent (string name, EventAgent _source, Commandlet _command, ISelectable _unit) : base("command" + name, _source) {
			Unit = _unit;
			Command = _command;
		}
	}
}