using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events.Selectable {

	public class SquadRegisterEvent : AbstractEvent {

		public ISelectable Host { get; private set; }

		public ISelectable RegisteredMember { get; private set; }

		public SquadRegisterEvent (EventAgent _source, ISelectable _host, ISelectable _member) : base("squadRegister", _source) {
			Host = _host;
			RegisteredMember = _member;
		}
	}
}