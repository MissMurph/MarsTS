using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Receivers;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandActiveEvent : CommandEvent 
	{
		public bool Activity { get; private set; }

		public CommandActiveEvent (ICommandable unit, ICommandReceiver commandReceiver, bool activity) 
			: base("Active", unit) 
		{
			Activity = activity;
		}
	}
}