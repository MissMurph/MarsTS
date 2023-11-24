using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public class PlaceBuilding : Command<ISelectable> {

		public override string Name { get { return "construct/" + building.UnitType; } }

		public override Sprite Icon { get { return building.Icon; } }

		[SerializeField]
		protected Building building;

		protected Transform ghostTransform;
		protected BuildingGhost ghostComp;

		protected CostEntry[] cost;

		private void Awake () {
			cost = building.ConstructionCost;
		}

		public override void StartSelection () {
			bool canAfford = true;

			foreach (CostEntry entry in cost) {
				if (Player.Main.Resource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (canAfford) {
				ghostTransform = Instantiate(building.SelectionGhost).transform;
				ghostComp = ghostTransform.GetComponent<BuildingGhost>();
				Player.Input.Hook("Select", OnSelect);
				Player.Input.Hook("Order", OnOrder);
			}
		}

		protected virtual void Update () {
			if (ghostTransform != null) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					ghostTransform.position = hit.point;
				}
			}
		}

		protected virtual void OnSelect (InputAction.CallbackContext context) {
			if (context.canceled) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					bool canAfford = true;

					foreach (CostEntry entry in cost) {
						if (Player.Main.Resource(entry.key).Amount < entry.amount) {
							canAfford = false;
							break;
						}
					}

					if (canAfford && ghostComp.Legal) {
						Building newBuilding = Instantiate(building, hit.point, Quaternion.Euler(Vector3.zero)).GetComponent<Building>();
						newBuilding.SetOwner(Player.Main);

						newBuilding.GetComponent<EventAgent>().AddListener<EntityInitEvent>((_event) => {
							Player.Main.DeliverCommand(CommandRegistry.Get<Repair>("repair").Construct(newBuilding), Player.Include);
						});

						Destroy(ghostTransform.gameObject);

						Player.Input.Release("Select");
						Player.Input.Release("Order");

						foreach (CostEntry entry in cost) {
							Player.Main.Resource(entry.key).Withdraw(entry.amount);
						}
					}
				}
			}
		}

		protected virtual void OnOrder (InputAction.CallbackContext context) {
			Destroy(ghostTransform.gameObject);

			Player.Input.Release("Select");
			Player.Input.Release("Order");
		}
	}
}