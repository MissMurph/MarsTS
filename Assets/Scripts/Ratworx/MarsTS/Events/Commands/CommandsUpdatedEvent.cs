using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Commands {

	public class CommandsUpdatedEvent : AbstractEvent {

		public string[] NewCommands { get; private set; }
		public Entity Unit { get; private set; }

		public CommandsUpdatedEvent (Entity unit, params string[] newCommands) : base("commandsUpdated") {
			NewCommands = newCommands;
			Unit = unit;
		}
	}
}