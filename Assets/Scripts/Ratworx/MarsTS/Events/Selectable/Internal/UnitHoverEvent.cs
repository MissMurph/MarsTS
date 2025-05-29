namespace Ratworx.MarsTS.Events.Selectable.Internal {

    public class UnitHoverEvent : AbstractEvent {

		public bool Status { get; private set; }

		public UnitHoverEvent (bool status) : base("unitHover") {
			Status = status;
		}
	}
}