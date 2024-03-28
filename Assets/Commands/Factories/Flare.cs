using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

    public class Flare : CommandFactory<Vector3> {

		public override string Name { get { return "flare"; } }

		public override Sprite Icon { get { return icon; } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		[SerializeField]
		private GameObject markerPrefab;
		private Transform markerTransform;

		[SerializeField]
		private float cooldown;

		[SerializeField]
		private CostEntry[] cost;

		public override void StartSelection () {
			bool canAfford = true;

			foreach (CostEntry entry in cost) {
				if (Player.Commander.Resource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (canAfford) {
				markerTransform = Instantiate(markerPrefab).transform;
				Player.Input.Hook("Select", OnSelect);
				Player.Input.Hook("Order", OnOrder);
			}
		}

		protected virtual void Update () {
			if (markerTransform != null) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					markerTransform.position = hit.point;
				}
			}
		}

		protected virtual void OnSelect (InputAction.CallbackContext context) {
			if (context.canceled) {
				Ray ray = Player.ViewPort.ScreenPointToRay(Player.MousePos);

				if (Physics.Raycast(ray, out RaycastHit hit, 1000f, GameWorld.WalkableMask)) {
					bool canAfford = true;

					foreach (CostEntry entry in cost) {
						if (Player.Commander.Resource(entry.key).Amount < entry.amount) {
							canAfford = false;
							break;
						}
					}

					if (canAfford) {
						//Player.Main.DistributeCommand(Construct(hit.point), Player.Include);

						Destroy(markerTransform.gameObject);

						Player.Input.Release("Select");
						Player.Input.Release("Order");

						foreach (CostEntry entry in cost) {
							Player.Commander.Resource(entry.key).Withdraw(entry.amount);
						}
					}
				}
			}
		}

		/*public override Commandlet Construct (Vector3 _target) {
			return new FlareCommandlet(Name, _target, Player.Commander, cooldown);
		}*/

		protected virtual void OnOrder (InputAction.CallbackContext context) {
			if (context.canceled) {
				CancelSelection();
			}
		}

		public override void CancelSelection () {
			if (markerTransform != null) {
				Destroy(markerTransform.gameObject);

				Player.Input.Release("Select");
				Player.Input.Release("Order");
			}
		}

		public override CostEntry[] GetCost () {
			List<CostEntry> spool = new List<CostEntry>();

			foreach (CostEntry entry in cost) {
				spool.Add(entry);
			}

			CostEntry time = new CostEntry();
			time.key = "time";
			time.amount = (int)cooldown;

			spool.Add(time);

			return spool.ToArray();
		}
	}

	public class FlareCommandlet : Commandlet<Vector3> {

		private float cooldown;

		public FlareCommandlet (string name, Vector3 target, Faction commander, float _cooldown) {
			cooldown = _cooldown;
		}

		public override string Key => Name;

		public override void OnComplete (CommandQueue queue, CommandCompleteEvent _event) {
			if (!_event.CommandCancelled) queue.Cooldown(this, cooldown);

			base.OnComplete(queue, _event);
		}
	}
}