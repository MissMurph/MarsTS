using MarsTS.Networking;
using MarsTS.Players;
using MarsTS.UI;
using MarsTS.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

    public class PlacePumpjack : PlaceBuilding {

        private PumpjackSnapping _snapper = null;

		[SerializeField]
		private GameObject snapPrefab;

		public override void StartSelection () {
			if (!CanFactionAfford(Player.Commander)) 
				return;
			
			base.StartSelection();

			_snapper = Instantiate(snapPrefab).GetComponent<PumpjackSnapping>();
		}

		protected override void Update () {
			if (GhostTransform is null || _snapper is null) 
				return;
			
			Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

			if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask))
				_snapper.gameObject.transform.position = hit.point;

			GhostTransform.position = _snapper.TrySnap(out Vector3 snapPos) ? snapPos : _snapper.transform.position;
		}
		
		protected override void OnSelect (InputAction.CallbackContext context) {
			if (!context.canceled) 
				return;
			
			if (!CanFactionAfford(Player.Commander) || !GhostComp.Legal) 
				return;
			
			PlaceBuildingServerRpc(
				GhostTransform.position,
				Quaternion.Euler(Vector3.zero),
				Player.Commander.Id,
				Player.ListSelected.ToNativeArray32(),
				Player.Include
			);
			
			Destroy(GhostTransform.gameObject);
			Destroy(_snapper.gameObject);
			
			Player.Input.Release("Select");
			Player.Input.Release("Order");
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

				Player.Input.Release("Select");
				Player.Input.Release("Order");
			}
		}
	}
}