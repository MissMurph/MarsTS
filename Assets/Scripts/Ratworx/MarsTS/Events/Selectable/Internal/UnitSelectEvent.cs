namespace Ratworx.MarsTS.Events.Selectable.Internal {

	public class UnitSelectEvent : AbstractEvent {

		public bool Status { get; private set; }

		public UnitSelectEvent (bool status) : base("unitSelect") {
			Status = status;
		}
	}
}