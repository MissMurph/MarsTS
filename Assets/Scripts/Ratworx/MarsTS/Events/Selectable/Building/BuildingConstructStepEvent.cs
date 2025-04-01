namespace Ratworx.MarsTS.Events.Selectable.Building {

	public class BuildingConstructStepEvent : AbstractEvent {

		public Buildings.Building Building {
			get {
				return building;
			}
		}

		private Buildings.Building building;

		public BuildingConstructStepEvent (EventAgent _source, Buildings.Building _building) : base("buildingConstructStep", _source) {
			building = _building;
		}
	}
}