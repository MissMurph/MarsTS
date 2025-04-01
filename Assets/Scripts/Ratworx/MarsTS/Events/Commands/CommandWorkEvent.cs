using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;

namespace Ratworx.MarsTS.Events.Commands 
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