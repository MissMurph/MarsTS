using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands 
{
	public class CommandEvent : AbstractEvent 
	{
		public Commandlet Command { get; private set; }
		public ICommandable Unit { get; private set; }

		protected CommandEvent(
			string name,
			Commandlet command,
			ICommandable unit
		) : base(
			"command" + name
		) {
			Unit = unit;
			Command = command;
		}
	}
}