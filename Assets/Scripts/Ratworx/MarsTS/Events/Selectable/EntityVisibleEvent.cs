using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Vision;

namespace Ratworx.MarsTS.Events.Selectable {

	public class EntityVisibleEvent : UnitEvent {

		public bool Visible { get; private set; }

		public EntityVisibleEvent (
			UnitVision unitVision,
			bool visible
		) : base(
			"Visible",
			unitVision.Entity
		) {
			Visible = visible;
		}
	}
}