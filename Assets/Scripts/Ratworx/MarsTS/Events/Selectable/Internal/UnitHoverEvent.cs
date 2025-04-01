namespace Ratworx.MarsTS.Events.Selectable.Internal {

    public class UnitHoverEvent : AbstractEvent {

		public bool Status { get; private set; }

		public UnitHoverEvent (EventAgent _source, bool status) : base("unitHover", _source) {
			Status = status;
		}
	}
}