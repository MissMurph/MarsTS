using System.Collections.Generic;
using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Player {

	public class PlayerSelectEvent : AbstractEvent {

		public Dictionary<string, Roster> Selected { get; private set; }

		public PlayerSelectEvent (Dictionary<string, Roster> selection) : base("playerSelect", MarsTS.Player.Player.EventAgent) {
			Selected = selection;
		}
	}
}