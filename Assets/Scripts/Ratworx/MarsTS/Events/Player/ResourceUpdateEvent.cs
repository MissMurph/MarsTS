using Ratworx.MarsTS.Player;
using Ratworx.MarsTS.Teams;

namespace Ratworx.MarsTS.Events.Player {

	public class ResourceUpdateEvent : AbstractEvent {

		public Faction Player { get; private set; }
		public int Amount { get { return Resource.Amount; } }
		public PlayerResource Resource { get; private set; }

		public ResourceUpdateEvent (EventAgent _source, Faction _player, PlayerResource _resource) : base("resourceBanked", _source) {
			Player = _player;
			Resource = _resource;
		}
	}
}