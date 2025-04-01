using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands {

	public class CooldownCompletedEvent : CommandEvent {

		public CooldownCompletedEvent (string name, EventAgent _source, Commandlet _command, ICommandable _unit) 
			: base(name, _source, _command, _unit) 
		{

		}
	}
}