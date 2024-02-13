using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CommandActiveEvent : CommandEvent {

		public bool Activity { get; private set; }

		public CommandActiveEvent (EventAgent _source, ISelectable _unit, Commandlet _command, bool _activity) : base("Active", _source, _command, _unit) {
			Activity = _activity;
		}
	}
}