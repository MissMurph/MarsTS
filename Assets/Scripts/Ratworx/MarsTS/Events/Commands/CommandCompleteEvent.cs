using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Receivers;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandCompleteEvent : CommandEvent 
	{
		public bool IsCancelled { get; private set; }
		public Commandlet Command { get; private set; }

		public CommandCompleteEvent (Commandlet command, ICommandReceiver receiver, bool cancelled, ICommandable unit) 
			: base("Completed", receiver, unit) 
		{
			IsCancelled = cancelled;
		}
	}
}