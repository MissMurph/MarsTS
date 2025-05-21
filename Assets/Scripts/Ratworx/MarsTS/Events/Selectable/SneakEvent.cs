using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Events.Selectable {

	public class SneakEvent : UnitEvent {

		public bool IsSneaking { get; private set; }

		public SneakEvent (Entity unit, bool isSneaking) : base("Sneak", unit) {
			IsSneaking = isSneaking;
		}
	}
}