using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandCompleteEvent : CommandEvent 
	{
		public bool IsCancelled { get; private set; }

		public CommandCompleteEvent (Commandlet _command, bool _cancelled, ICommandable _unit) 
			: base("Completed", _command, _unit) 
		{
			IsCancelled = _cancelled;
		}
	}
}