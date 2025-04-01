using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Infantry;

namespace Ratworx.MarsTS.Commands {

    public class SquadQueue : CommandQueue {

		protected InfantrySquad parentSquad;

		protected override void Awake () {
			base.Awake();

			parentSquad = orderSource as InfantrySquad;
		}

		protected override void OnOrderComplete (CommandCompleteEvent _event) {
			if (!parentSquad.Members.Contains(_event.Unit as ISelectable)) return;
			Current = null;
			bus.Global(_event);
		}
	}
}