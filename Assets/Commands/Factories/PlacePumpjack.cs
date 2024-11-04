using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.UI;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

    public class PlacePumpjack : PlaceBuilding {

        private PumpjackSnapping snapper;
        private Transform snapTransform;

		[SerializeField]
		private GameObject snapPrefab;

		public override void StartSelection () {
			bool canAfford = true;

			foreach (CostEntry entry in cost) {
				if (Player.Commander.GetResource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (canAfford) {
				ghostTransform = Instantiate(building.SelectionGhost).transform;
				ghostComp = ghostTransform.GetComponent<BuildingGhost>();

				snapTransform = Instantiate(snapPrefab).transform;
				snapper = snapTransform.GetComponent<PumpjackSnapping>();

				Player.Input.Hook("Select", OnSelect);
				Player.Input.Hook("Order", OnOrder);
			}
		}

		protected override void Update () {
			if (ghostTransform != null) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					snapTransform.position = hit.point;
				}

				if (snapper.Snap != null) {
					ghostTransform.position = snapper.Snap.transform.position;
				}
				else {
					ghostTransform.position = snapTransform.position;
				}
			}
		}

		protected override void OnSelect (InputAction.CallbackContext context) {
			if (context.canceled && ghostComp.Legal) {
				bool canAfford = true;

				foreach (CostEntry entry in cost) {
					if (Player.Commander.GetResource(entry.key).Amount < entry.amount) {
						canAfford = false;
						break;
					}
				}

				if (canAfford && ghostComp.Legal) {
					Building newBuilding = Instantiate(building, snapper.Snap.transform.position, Quaternion.Euler(Vector3.zero)).GetComponent<Building>();
					newBuilding.SetOwner(Player.Commander);

					newBuilding.GetComponent<EventAgent>().AddListener<EntityInitEvent>((_event) => {
						if (_event.Phase == Phase.Pre) return;
						//Player.Main.DeliverCommand(CommandRegistry.Get<Repair>("repair").Construct(newBuilding), Player.Include);
					});

					Destroy(ghostTransform.gameObject);
					Destroy(snapTransform.gameObject);

					Player.Input.Release("Select");
					Player.Input.Release("Order");

					foreach (CostEntry entry in cost) {
						Player.Commander.GetResource(entry.key).Withdraw(entry.amount);
					}
				}
			}
		}

		protected override void OnOrder (InputAction.CallbackContext context) {
			if (context.canceled) {
				CancelSelection();
			}
		}

		public override void CancelSelection () {
			if (ghostTransform != null) {
				Destroy(ghostTransform.gameObject);
				Destroy(snapTransform.gameObject);

				Player.Input.Release("Select");
				Player.Input.Release("Order");
			}
		}
	}
}