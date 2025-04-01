using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Commands {

	public class CommandsUpdatedEvent : AbstractEvent {

		public string[] NewCommands { get; private set; }
		public ISelectable Unit { get; private set; }

		public CommandsUpdatedEvent (EventAgent _source, ISelectable _unit, params string[] _newCommands) : base("commandsUpdated", _source) {
			NewCommands = _newCommands;
			Unit = _unit;
		}
	}
}