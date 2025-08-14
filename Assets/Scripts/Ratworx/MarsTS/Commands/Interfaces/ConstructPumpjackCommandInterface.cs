using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Extensions;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Interfaces {

    public class ConstructPumpjackCommandInterface : ConstructBuildingCommandInterface {

        private PumpjackSnapping _snapper = null;

		[SerializeField]
		private GameObject snapPrefab;

		public override void StartArgSelection(ConstructionOption arg) {
			if (!arg.CanFactionAfford(Player.Player.Commander)) 
				return;
			
			base.StartArgSelection(arg);
			
			_snapper = Instantiate(snapPrefab).GetComponent<PumpjackSnapping>();
		}

		protected override void Update () {
			if (GhostTransform is null || _snapper is null) 
				return;
			
			Ray ray = Player.Player.ViewPort.ScreenPointToRay(Player.Player.MousePos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask))
				_snapper.gameObject.transform.position = hit.point;

			GhostTransform.position = _snapper.TrySnap(out Vector3 snapPos) ? snapPos : _snapper.transform.position;
		}
		
		protected override void OnSelect (InputAction.CallbackContext context) {
			if (!context.canceled) 
				return;
			
			if (!CurrentlyPlacingOption.CanFactionAfford(Player.Player.Commander) || !SelectionGhostComp.Legal) 
				return;
			
			base.OnSelect(context);
			
			Destroy(_snapper.gameObject);
			_snapper = null;
		}

		protected override void OnOrder (InputAction.CallbackContext context) {
			if (context.canceled) {
				CancelSelection();
			}
		}

		public override void CancelSelection () {
			if (GhostTransform != null) {
				Destroy(GhostTransform.gameObject);
				Destroy(_snapper.gameObject);
				_snapper = null;
				
				Player.Player.Input.Release("Select");
				Player.Player.Input.Release("Order");
			}
		}
	}
}