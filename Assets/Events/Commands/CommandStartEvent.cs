using MarsTS.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CommandStartEvent : AbstractEvent {

		public Commandlet Command { get; private set; }

		public CommandStartEvent (EventAgent _source, Commandlet _command) : base("commandStarted", _source) {
			Command = _command;
		}
	}
}