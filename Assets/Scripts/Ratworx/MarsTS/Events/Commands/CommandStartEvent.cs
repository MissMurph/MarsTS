using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands
{
	//Stinky
	public class CommandStartEvent : CommandEvent 
	{
		public CommandStartEvent (EventAgent _source, Commandlet _command, ICommandable _unit) 
			: base("Started", _source, _command, _unit) 
		{
			
		}
	}
}