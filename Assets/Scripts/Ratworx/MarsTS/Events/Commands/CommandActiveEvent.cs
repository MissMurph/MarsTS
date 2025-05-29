using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandActiveEvent : CommandEvent 
	{
		public bool Activity { get; private set; }

		public CommandActiveEvent (ICommandable unit, Commandlet command, bool activity) 
			: base("Active", command, unit) 
		{
			Activity = activity;
		}
	}
}