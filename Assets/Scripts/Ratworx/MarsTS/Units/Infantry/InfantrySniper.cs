using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.Turrets;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry {

    public class InfantrySniper : AbstractUnit {

		[SerializeField] protected float baseSpeed;
		[SerializeField] protected float currentSpeed;
		[SerializeField] private float sneakSpeed;
		[SerializeField] private float flareRange;
		[SerializeField] private GameObject flarePrefab;

		protected virtual void FixedUpdate () {
			if (!NetworkManager.Singleton.IsServer) return;
			
			if (ground.Grounded) {
				//Dunno why we need this check on the infantry member when we don't need it on any other unit type...
				if (!CurrentPath.IsEmpty && !(PathIndex >= CurrentPath.Length)) {
					Vector3 targetWaypoint = CurrentPath[PathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;
					Body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z));

					Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					Vector3 newVelocity = moveDirection * currentSpeed;

					Body.velocity = newVelocity;
				}
				else {
					Body.velocity = Vector3.zero;
				}
			}
		}

		/*	Sneak	*/
		private void Sneak (Commandlet order) {
			Commandlet<bool> deserialized = order as Commandlet<bool>;

			if (deserialized.Target) {
				isSneaking = true;
				currentSpeed = sneakSpeed;
			}
			else {
				isSneaking = false;
				currentSpeed = baseSpeed;
			}

			commands.Activate(order, deserialized.Target);

			Bus.PostLocal(new SneakEvent(Bus, this, isSneaking));
			
			PostSneakEventClientRpc(isSneaking);
		}
		
		[Rpc(SendTo.NotServer)]
		private void PostSneakEventClientRpc(bool status) => Bus.PostLocal(new SneakEvent(Bus, this, status));

		/*	Flare	*/
		private void Flare (Commandlet order) {
			Commandlet<Vector3> deserialized = order as Commandlet<Vector3>;

			flareTarget = deserialized.Target;

			SetTarget(flareTarget);
		}

		private void FireFlare (Vector3 position) {
			Flare firedFlare = Instantiate(flarePrefab, position, Quaternion.Euler(Vector3.zero)).GetComponent<Flare>();
			NetworkObject networkObject = firedFlare.GetComponent<NetworkObject>();
			
			networkObject.Spawn();
			firedFlare.SetOwner(Owner);
			
			CurrentCommand.CompleteCommand(Bus, this);

			Stop();
		}
	}
}