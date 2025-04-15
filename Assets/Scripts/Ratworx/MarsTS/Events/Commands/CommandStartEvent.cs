using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands
{
	//Stinky
	public class CommandStartEvent : CommandEvent 
	{
		public CommandStartEvent (Commandlet _command, ICommandable _unit) 
			: base("Started", _command, _unit) 
		{
			
		}
	}
}