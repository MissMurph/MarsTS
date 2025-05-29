using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Receivers;

namespace Ratworx.MarsTS.Events.Commands {

	public class CooldownCompletedEvent : CommandEvent {
		public CooldownCompletedEvent (ICommandReceiver receiver, ICommandable unit) 
			: base("CooldownCompleted", receiver, unit) 
		{

		}
	}
}