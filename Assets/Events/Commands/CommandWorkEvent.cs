using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events 
{
	public class CommandWorkEvent : CommandEvent 
	{
		public string CommandName { get; private set; }
		public IWorkable Work { get; private set; }

		public CommandWorkEvent (EventAgent _source, Commandlet _command, ICommandable _unit, IWorkable _work) 
			: base("Work", _source, _command, _unit) 
		{
			CommandName = _command.Name;
			Work = _work;
		}
	}
}