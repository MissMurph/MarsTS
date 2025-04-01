using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandCompleteEvent : CommandEvent 
	{
		public bool IsCancelled { get; private set; }

		public CommandCompleteEvent (EventAgent _source, Commandlet _command, bool _cancelled, ICommandable _unit) 
			: base("Completed", _source, _command, _unit) 
		{
			IsCancelled = _cancelled;
		}
	}
}