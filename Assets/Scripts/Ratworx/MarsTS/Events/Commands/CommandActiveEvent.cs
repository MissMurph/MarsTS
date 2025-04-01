using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandActiveEvent : CommandEvent 
	{
		public bool Activity { get; private set; }

		public CommandActiveEvent (EventAgent _source, ICommandable _unit, Commandlet _command, bool _activity) 
			: base("Active", _source, _command, _unit) 
		{
			Activity = _activity;
		}
	}
}