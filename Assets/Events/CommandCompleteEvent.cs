using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CommandCompleteEvent : AbstractEvent {

		public Commandlet Command { get; private set; }
		public bool CommandCancelled { get; private set; }
		public ISelectable Unit { get; private set; }

		public CommandCompleteEvent (EventAgent _source, Commandlet _command, bool _cancelled, ISelectable _unit) : base("commandCompleted", _source) {
			Command = _command;
			CommandCancelled = _cancelled;
			Unit = _unit;
		}
	}
}