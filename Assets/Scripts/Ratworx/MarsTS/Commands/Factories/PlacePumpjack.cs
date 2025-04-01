using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ratworx.MarsTS.Commands.Factories {

    public class PlacePumpjack : PlaceBuilding {

        private PumpjackSnapping _snapper = null;

		[SerializeField]
		private GameObject snapPrefab;

		public override void StartSelection () {
			if (!CanFactionAfford(Player.Player.Commander)) 
				return;
			
			base.StartSelection();

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
			
			if (!CanFactionAfford(Player.Player.Commander) || !SelectionGhostComp.Legal) 
				return;
			
			PlaceBuildingServerRpc(
				GhostTransform.position,
				Quaternion.Euler(Vector3.zero),
				Player.Player.Commander.Id,
				Player.Player.ListSelected.ToNativeArray32(),
				Player.Player.Include
			);
			
			Destroy(GhostTransform.gameObject);
			Destroy(_snapper.gameObject);
			_snapper = null;
			
			Player.Player.Input.Release("Select");
			Player.Player.Input.Release("Order");
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