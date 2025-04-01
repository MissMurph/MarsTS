using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Units {

    public class InfantrySniper : AbstractUnit {

		[SerializeField]
		protected float baseSpeed;

		[SerializeField]
		protected float currentSpeed;

		[SerializeField]
		private float sneakSpeed;

		[SerializeField]
		private float flareRange;

		[SerializeField]
		private GameObject flarePrefab;

		protected GroundDetection ground;
		
		protected UnitReference<IAttackable> AttackTarget = new UnitReference<IAttackable>();

		protected Vector3 flareTarget;

		private ProjectileTurret equippedWeapon;

		protected bool isSneaking;

		protected override void Awake () {
			base.Awake();

			ground = GetComponent<GroundDetection>();

			currentSpeed = baseSpeed;

			equippedWeapon = GetComponentInChildren<ProjectileTurret>();
		}

		protected override void ServerUpdate () {
			//I'd like to move these all to commands, for now they'll remain here
			//Will start devising a method to do so
			if (AttackTarget.Get != null) {
				if (equippedWeapon.IsInRange(AttackTarget.Get)) {
					TrackedTarget = null;
					CurrentPath = Path.Empty;
				}
				else if (!ReferenceEquals(TrackedTarget, AttackTarget.GameObject.transform)) {
					SetTarget(AttackTarget.GameObject.transform);
				}
			}

			if (CurrentCommand != null && CurrentCommand.Name == "flare") {
				if ((flareTarget - transform.position).sqrMagnitude < (flareRange * flareRange)) {
					TrackedTarget = null;
					CurrentPath = Path.Empty;

					FireFlare(flareTarget);
				}
				else {
					SetTarget(flareTarget);
				}
			}
		}

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

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "attack":
					break;
				case "sneak":
					Sneak(order);
					break;
				case "flare":
					break;
				default:
					base.Order(order, inclusive);
					return;
			}

			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}

		protected override void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "attack":
					Attack(_event.Command);
					break;
				case "flare":
					Flare(_event.Command);
					break;
				default:
					base.ExecuteOrder(_event);
					break;
			}
		}

		/*	Commands	*/

		/*	Attack	*/
		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget.Set(deserialized.Target, deserialized.Target.GameObject);

				EntityCache.TryGet(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				AttackTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, true, this);

			CurrentCommand.Callback.Invoke(newEvent);

			Stop();
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

			Bus.Local(new SneakEvent(Bus, this, isSneaking));
			
			PostSneakEventClientRpc(isSneaking);
		}
		
		[Rpc(SendTo.NotServer)]
		private void PostSneakEventClientRpc(bool status) => Bus.Local(new SneakEvent(Bus, this, status));

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

		public override CommandFactory Evaluate (ISelectable target) {
			if (target is IAttackable && target.GetRelationship(Owner) == Relationship.Hostile) {
				return CommandPrimer.Get("attack");
			}

			return CommandPrimer.Get("move");
		}

		public override void AutoCommand (ISelectable target) {
			if (target is IAttackable attackable && target.GetRelationship(Owner) == Relationship.Hostile) {
				CommandPrimer.Get<Attack>("attack").Construct(attackable);
				return;
			}

			CommandPrimer.Get<Move>("move").Construct(target.GameObject.transform.position);
		}
	}
}