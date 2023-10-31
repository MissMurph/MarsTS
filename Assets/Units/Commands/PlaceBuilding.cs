using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Players;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Units.Commands {

	public class PlaceBuilding : Command<ISelectable> {

		public override string Name { get { return "construct/" + building.BuildingType; } }

		[SerializeField]
		private Building building;

		private Transform ghostTransform;

		public override void StartSelection () {
			ghostTransform = Instantiate(building.SelectionGhost).transform;

			Player.Input.Hook("Select", OnSelect);
			Player.Input.Hook("Order", OnOrder);
		}

		private void Update () {
			if (ghostTransform != null) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					ghostTransform.position = hit.point;
				}
			}
		}

		private void OnSelect (InputAction.CallbackContext context) {
			if (context.canceled) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					Building newBuilding = Instantiate(building, hit.point, Quaternion.Euler(Vector3.zero)).GetComponent<Building>();
					newBuilding.SetOwner(Player.Main);

					Destroy(ghostTransform.gameObject);

					Player.Main.DeliverCommand(Commands.Get<Repair>("repair").Construct(newBuilding), Player.Include);

					Player.Input.Release("Select");
					Player.Input.Release("Order");
				}
			}
		}

		private void OnOrder (InputAction.CallbackContext context) {
			Destroy(ghostTransform.gameObject);

			Player.Input.Release("Select");
			Player.Input.Release("Order");
		}
	}
}