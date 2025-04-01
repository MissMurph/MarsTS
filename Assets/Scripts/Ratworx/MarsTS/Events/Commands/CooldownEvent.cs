using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Commands {

	public class CooldownEvent : AbstractEvent {

		public string CommandKey { get; private set; }
		public ISelectable Unit { get; private set; }
		public Timer Cooldown { get; private set; }
		public bool Complete { get { return Cooldown.timeRemaining <= 0f; } }

		public CooldownEvent (EventAgent _source, string _command, ISelectable _unit, Timer _cooldown) : base("cooldown", _source) {
			Cooldown = _cooldown;
			CommandKey = _command;
			Unit = _unit;
		}
	}
}