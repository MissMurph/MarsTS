namespace Ratworx.MarsTS.Events.Selectable {

	public class BuildingVisibleEvent : EntityVisibleEvent {

		public bool Visited { get; private set; }

		public BuildingVisibleEvent (EventAgent _source, Buildings.Building _building, bool _visible, bool _visited) : base(_source, _building, _visible) {
			Visited = _visited;
		}
	}
}