using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Units.Turrets;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry
{
	public class Roughneck : InfantryMember
	{

		/*	Infantry Fields	*/

		[SerializeField] private float sneakSpeed;
		private bool isSneaking;

		protected RoughneckSquad roughneckSquad;

		protected void Awake() {
			// equippedWeapon = GetComponentInChildren<ProjectileTurret>();
		}

		public override void OnNetworkSpawn() {
			base.OnNetworkSpawn();

			_entityComponent.OnEntityInit += OnEntityInit;

			if (!NetworkManager.Singleton.IsServer) return;
		}

		private void OnEntityInit(Phase phase) {
			if (phase == Phase.Post
				|| _squad == null) return;

			roughneckSquad = _squad as RoughneckSquad;
		}

		/*	Sneak	*/
		private void Sneak(Commandlet order) {
			Commandlet<bool> deserialized = order as Commandlet<bool>;
			if (deserialized.Target) {
				isSneaking = true;
				_currentSpeed = sneakSpeed;
			}
			else {
				isSneaking = false;
				_currentSpeed = _moveSpeed;
			}

			_bus.PostLocal(new SneakEvent(_bus, this, isSneaking));

			PostSneakEventClientRpc(isSneaking);
		}

		[Rpc(SendTo.NotServer)]
		private void PostSneakEventClientRpc(bool status) => _bus.PostLocal(new SneakEvent(_bus, this, status));
	}
}