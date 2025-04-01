using Ratworx.MarsTS.Commands;

namespace Ratworx.MarsTS.Events.Commands {

	public class CommandEvent : AbstractEvent {

		public Commandlet Command { get; private set; }
		public ICommandable Unit { get; private set; }

		protected CommandEvent (string name, EventAgent _source, Commandlet _command, ICommandable _unit) : base("command" + name, _source) {
			Unit = _unit;
			Command = _command;
		}
	}
}