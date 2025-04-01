using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CooldownCompletedEvent : CommandEvent {

		public CooldownCompletedEvent (string name, EventAgent _source, Commandlet _command, ICommandable _unit) 
			: base(name, _source, _command, _unit) 
		{

		}
	}
}