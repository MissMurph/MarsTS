using Ratworx.MarsTS.Player;
using Ratworx.MarsTS.Teams;

namespace Ratworx.MarsTS.Events.Player {

	public class ResourceUpdateEvent : AbstractEvent {

		public Faction Player { get; private set; }
		public int Amount => Resource.Amount;
		public PlayerResource Resource { get; private set; }

		public ResourceUpdateEvent (Faction player, PlayerResource resource) : base("resourceBanked") {
			Player = player;
			Resource = resource;
		}
	}
}