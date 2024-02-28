using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class InfantrySniper : Unit {

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

		protected override void Update () {
			base.Update();

			//I'd like to move these all to commands, for now they'll remain here
			//Will start devising a method to do so
			if (AttackTarget.Get != null) {
				if (equippedWeapon.IsInRange(AttackTarget.Get)) {
					TrackedTarget = null;
					currentPath = Path.Empty;
				}
				else if (!ReferenceEquals(TrackedTarget, AttackTarget.GameObject.transform)) {
					SetTarget(AttackTarget.GameObject.transform);
				}
			}

			/*if (flareTarget != null) {
				if ((flareTarget - transform.position).sqrMagnitude < (flareRange * flareRange)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					FireFlare(flareTarget);
				}
				else {
					SetTarget(flareTarget);
				}
			}*/
		}

		protected virtual void FixedUpdate () {
			if (ground.Grounded) {
				//Dunno why we need this check on the infantry member when we don't need it on any other unit type...
				if (!currentPath.IsEmpty && !(pathIndex >= currentPath.Length)) {
					Vector3 targetWaypoint = currentPath[pathIndex];

					Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
					float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;
					body.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z));

					Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, ground.Slope.normal);

					Vector3 newVelocity = moveDirection * currentSpeed;

					body.velocity = newVelocity;
				}
				else {
					body.velocity = Vector3.zero;
				}
			}
		}

		public override void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

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

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, true, this);

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

			bus.Local(new SneakEvent(bus, this, isSneaking));
		}

		/*	Flare	*/
		private void Flare (Commandlet order) {
			Commandlet<Vector3> deserialized = order as Commandlet<Vector3>;

			flareTarget = deserialized.Target;

			SetTarget(flareTarget);
		}

		private void FireFlare (Vector3 position) {
			Instantiate(flarePrefab);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, true, this);

			CurrentCommand.OnComplete(commands, newEvent);
		}

		public override Command Evaluate (ISelectable target) {
			if (target is IAttackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get("attack");
			}

			return CommandRegistry.Get("move");
		}

		public override Commandlet Auto (ISelectable target) {
			if (target is IAttackable attackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get<Attack>("attack").Construct(attackable);
			}

			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}
	}
}