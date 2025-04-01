using Ratworx.MarsTS.Units;

namespace Ratworx.MarsTS.Events {

    public class WorkEvent : AbstractEvent {

		public ISelectable Unit { get; private set; }
		public int WorkRequired { get; private set; }
		public int CurrentWork { get; private set; }

		public WorkEvent (EventAgent _source, ISelectable _unit, int _workRequired, int _currentWork) : base("work", _source) {
			Unit = _unit;
			WorkRequired = _workRequired;
			CurrentWork = _currentWork;
		}
	}
}