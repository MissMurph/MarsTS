using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CommandWorkEvent : AbstractEvent {

		public string CommandKey { get; private set; }
		public ISelectable Unit { get; private set; }
		public IWorkable Work { get; private set; }

		public CommandWorkEvent (EventAgent _source, string _command, ISelectable _unit, IWorkable _work) : base("commandWork", _source) {
			CommandKey = _command;
			Unit = _unit;
			Work = _work;
		}
	}
}