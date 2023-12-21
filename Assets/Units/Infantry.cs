using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MarsTS.Units {

	public class Infantry : Unit {

		public new Faction Owner { get { return squad.Owner; } }

		public InfantrySquad squad;

		[SerializeField]
		private float moveSpeed;

		[SerializeField]
		private float sneakSpeed;

		private float currentSpeed;

		private GroundDetection ground;

		private bool isSneaking;

		private bool isSelected;

		private ProjectileTurret equippedWeapon;

		//How many units per second
		[SerializeField]
		private float repairRate;

		private int repairAmount;
		protected float cooldown;
		protected float currentCooldown;

		protected IAttackable AttackTarget {
			get {
				return attackTarget;
			}
			set {
				if (attackTarget != null) {
					EntityCache.TryGet(attackTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => AttackTarget = null);
					oldAgent.RemoveListener<UnitVisibleEvent>((_event) => {
						if (!_event.Visible) {
							SetTarget(AttackTarget.GameObject.transform.position);
							AttackTarget = null;
						}
					});
				}

				attackTarget = value;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => AttackTarget = null);
					agent.AddListener<UnitVisibleEvent>((_event) => {
						if (!_event.Visible) {
							SetTarget(AttackTarget.GameObject.transform.position);
							AttackTarget = null;
						}
					});
				}
			}
		}

		protected IAttackable attackTarget;

		protected IAttackable RepairTarget {
			get {
				return repairTarget;
			}
			set {
				if (repairTarget != null) {
					EntityCache.TryGet(repairTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => repairTarget = null);
				}

				repairTarget = value;

				if (value != null) {
					AttackTarget = null;

					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => repairTarget = null);
				}
			}
		}

		protected IAttackable repairTarget;

		protected AttackableSensor repairSensor;

		protected override void Awake () {
			base.Awake();

			ground = GetComponent<GroundDetection>();
			equippedWeapon = GetComponentInChildren<ProjectileTurret>();
			repairSensor = transform.Find("RepairRange").GetComponent<AttackableSensor>();

			currentSpeed = moveSpeed;
			isSelected = false;

			cooldown = 1f / repairRate;
			repairAmount = Mathf.RoundToInt(repairRate * cooldown);
			currentCooldown = cooldown;
		}

		protected override void Update () {
			base.Update();

			if (currentCooldown >= 0f) {
				currentCooldown -= Time.deltaTime;
			}
			

			if (attackTarget != null) {
				if (equippedWeapon.IsInRange(AttackTarget)) {
					TrackedTarget = null;
					currentPath = Path.Empty;
				}
				else if (!ReferenceEquals(TrackedTarget, AttackTarget.GameObject.transform)) {
					SetTarget(AttackTarget.GameObject.transform);
				}
			}

			if (repairTarget != null) {
				if (repairSensor.IsDetected(RepairTarget)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					if (currentCooldown <= 0f) FireRepair();
				}
				else if (!ReferenceEquals(TrackedTarget, RepairTarget.GameObject.transform)) {
					SetTarget(RepairTarget.GameObject.transform);
				}
			}
		}

		protected virtual void FixedUpdate () {
			if (ground.Grounded) {
				if (!currentPath.IsEmpty) {
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

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "sneak":
					Sneak();
					break;
				case "attack":
					CurrentCommand = order;
					Attack(order);
					break;
				case "repair":
					CurrentCommand = order;
					Repair(order);
					break;
				default:
					base.ProcessOrder(order);
					break;
			}
		}

		public override void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			if (order.Name == "sneak") {
				Sneak();
				return;
			}

			commandQueue.Clear();

			currentPath = Path.Empty;
			TrackedTarget = null;

			if (CurrentCommand != null) {
				CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, true, this);
				CurrentCommand.Callback.Invoke(_event);
				bus.Global(_event);
			}

			CurrentCommand = null;
			commandQueue.Enqueue(order);
		}

		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget = deserialized.Target;

				EntityCache.TryGet(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<EntityDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		private void Sneak () {
			if (isSneaking) {
				isSneaking = false;
				currentSpeed = moveSpeed;
			}
			else {
				isSneaking = true;
				currentSpeed = sneakSpeed;
			}

			bus.Local(new SneakEvent(bus, this, isSneaking));
		}

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			//bus.RemoveListener<CommandCompleteEvent>(AttackCancelled);

			if (_event.Command is Commandlet<IAttackable> deserialized && _event.CommandCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

				AttackTarget = null;
			}
		}

		private void OnTargetDeath (EntityDeathEvent _event) {
			EntityCache.TryGet(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, true, this);

			CurrentCommand.Callback.Invoke(newEvent);

			bus.Global(newEvent);

			CurrentCommand = null;

			Stop();
		}

		protected void Repair (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				IAttackable unit = deserialized.Target;

				if (unit.GetRelationship(owner) == Relationship.Owned || unit.GetRelationship(owner) == Relationship.Friendly) {
					RepairTarget = unit;

					EntityCache.TryGet(RepairTarget.GameObject.transform.root.name, out EventAgent targetBus);

					targetBus.AddListener<UnitHurtEvent>(OnTargetHealed);
					targetBus.AddListener<EntityDeathEvent>(OnTargetDeath);

					order.Callback.AddListener(RepairCancelled);
				}
			}
		}

		private void RepairCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized && _event.CommandCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);
				targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

				RepairTarget = null;
			}
		}

		//Could potentially move these to the actual Command Classes
		private void OnTargetHealed (UnitHurtEvent _event) {
			if (_event.Targetable.Health >= _event.Targetable.MaxHealth) {
				EntityCache.TryGet(_event.Targetable.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				bus.Global(newEvent);

				CurrentCommand = null;
				RepairTarget = null;
			}
		}

		private void FireRepair () {
			RepairTarget.Attack(-repairAmount);
			currentCooldown += cooldown;
		}

		public override void Select (bool status) {
			//selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
			isSelected = status;
		}

		public override void Hover (bool status) {
			//These are seperated due to the Player Selection Check
			if (status) {
				//selectionCircle.SetActive(true);
				bus.Local(new UnitHoverEvent(bus, status));
			}
			else if (!isSelected) {
				//selectionCircle.SetActive(false);
				bus.Local(new UnitHoverEvent(bus, status));
			}
		}

		public override Commandlet Auto (ISelectable target) {
			throw new System.NotImplementedException();
		}

		public override Command Evaluate (ISelectable target) {
			throw new System.NotImplementedException();
		}
	}
}