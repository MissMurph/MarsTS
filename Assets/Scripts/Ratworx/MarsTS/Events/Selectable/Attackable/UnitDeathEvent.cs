using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Selectable.Attackable {

	public class UnitDeathEvent : UnitEvent {
		public UnitDeathEvent (Entity unit) : base("Death", unit) {
		}
	}
}