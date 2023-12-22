using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;
using UnityEngine.SocialPlatforms.Impl;
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
		protected float repairCooldown;
		protected float currentRepairCooldown;

		/*	Attacking	*/

		protected UnitReference<IAttackable> AttackTarget = new UnitReference<IAttackable>();

		/*	Repairing	*/

		protected UnitReference<IAttackable> RepairTarget = new UnitReference<IAttackable>();

		protected AttackableSensor repairSensor;

		/*	Harvesting	*/

		protected UnitReference<IHarvestable> HarvestTarget = new UnitReference<IHarvestable>();

		private HarvestSensor harvestableDetector;

		//This is how many units per second are harvested
		[SerializeField]
		private float harvestRate;

		private int harvestAmount;
		private float harvestCooldown;
		private float currentHarvestCooldown;

		/*	Depositing	*/

		protected UnitReference<IDepositable> DepositTarget = new UnitReference<IDepositable>();

		protected DepositSensor depositableDetector;

		[SerializeField]
		protected float depositRate;

		protected int depositAmount;
		protected float depositCooldown;
		protected float currentDepositCooldown;

		protected override void Awake () {
			base.Awake();

			ground = GetComponent<GroundDetection>();
			equippedWeapon = GetComponentInChildren<ProjectileTurret>();
			repairSensor = transform.Find("InteractRange").GetComponent<AttackableSensor>();
			harvestableDetector = repairSensor.GetComponent<HarvestSensor>();
			depositableDetector = repairSensor.GetComponent<DepositSensor>();

			currentSpeed = moveSpeed;
			isSelected = false;

			repairCooldown = 1f / repairRate;
			repairAmount = Mathf.RoundToInt(repairRate * repairCooldown);
			currentRepairCooldown = repairCooldown;

			harvestCooldown = 1f / harvestRate;
			harvestAmount = Mathf.RoundToInt(harvestRate * harvestCooldown);
			currentHarvestCooldown = harvestCooldown;

			depositCooldown = 1f / depositRate;
			depositAmount = Mathf.RoundToInt(depositRate * depositCooldown);
			currentDepositCooldown = depositCooldown;
		}

		protected override void Update () {
			base.Update();

			if (currentRepairCooldown >= 0f) {
				currentRepairCooldown -= Time.deltaTime;
			}
			
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

			if (RepairTarget.Get != null) {
				if (repairSensor.IsDetected(RepairTarget.Get)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					if (currentRepairCooldown <= 0f) FireRepair();
				}
				else if (!ReferenceEquals(TrackedTarget, RepairTarget.GameObject.transform)) {
					SetTarget(RepairTarget.GameObject.transform);
				}
			}

			if (DepositTarget.Get != null) {
				if (depositableDetector.IsDetected(DepositTarget.Get)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					if (currentRepairCooldown <= 0f) DepositResources();

					currentRepairCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, DepositTarget.GameObject.transform)) {
					SetTarget(DepositTarget.GameObject.transform);
				}

				return;
			}

			if (HarvestTarget.Get != null) {
				if (harvestableDetector.IsDetected(HarvestTarget.Get)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					if (currentHarvestCooldown <= 0f) SiphonOil();

					currentHarvestCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, HarvestTarget.GameObject.transform)) {
					SetTarget(HarvestTarget.GameObject.transform);
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
				case "harvest":
					CurrentCommand = order;
					Harvest(order);
					break;
				case "deposit":
					CurrentCommand= order;
					Deposit(order);
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

		/*	Commands	*/

		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget.Set(deserialized.Target, deserialized.Target.GameObject);

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

		protected void Repair (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				IAttackable unit = deserialized.Target;

				if (unit.GetRelationship(owner) == Relationship.Owned || unit.GetRelationship(owner) == Relationship.Friendly) {
					RepairTarget.Set(unit, unit.GameObject);

					EntityCache.TryGet(RepairTarget.GameObject.transform.root.name, out EventAgent targetBus);

					targetBus.AddListener<UnitHurtEvent>(OnTargetHealed);
					targetBus.AddListener<EntityDeathEvent>(OnTargetDeath);

					order.Callback.AddListener(RepairCancelled);
				}
			}
		}

		private void Harvest (Commandlet order) {
			if (order is Commandlet<IHarvestable> deserialized) {
				HarvestTarget.Set(deserialized.Target, deserialized.Target.GameObject);

				bus.AddListener<ResourceHarvestedEvent>(OnExtraction);

				EntityCache.TryGet(HarvestTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<EntityDeathEvent>(OnDepositDepleted);

				order.Callback.AddListener(HarvestCancelled);
			}
		}

		private void Deposit (Commandlet order) {
			if (order is Commandlet<IDepositable> deserialized) {
				DepositTarget.Set(deserialized.Target, deserialized.Target.GameObject);
				TrackedTarget = deserialized.Target.GameObject.transform;

				bus.AddListener<HarvesterDepositEvent>(OnDeposit);

				order.Callback.AddListener(DepositCancelled);
			}
		}

		/*	Command Safety	*/

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			//bus.RemoveListener<CommandCompleteEvent>(AttackCancelled);

			if (_event.Command is Commandlet<IAttackable> deserialized && _event.CommandCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

				AttackTarget.Set(null, null);
				TrackedTarget = null;
			}
		}
		private void RepairCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized && _event.CommandCancelled) {
				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);
				targetBus.RemoveListener<EntityDeathEvent>(OnTargetDeath);

				RepairTarget.Set(null, null);
				TrackedTarget = null;
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

		//Could potentially move these to the actual Command Classes
		private void OnTargetHealed (UnitHurtEvent _event) {
			if (_event.Targetable.Health >= _event.Targetable.MaxHealth) {
				EntityCache.TryGet(_event.Targetable.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				bus.Global(newEvent);

				CurrentCommand = null;
				RepairTarget.Set(null, null);
			}
		}

		private void OnExtraction (ResourceHarvestedEvent _event) {
			if (squad.Stored >= squad.Capacity) {
				bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

				EntityCache.TryGet(_event.Deposit.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnDepositDepleted);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				bus.Global(newEvent);

				CurrentCommand = null;
			}
		}

		private void OnDeposit (HarvesterDepositEvent _event) {
			if (squad.Stored <= 0) {
				bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				bus.Global(newEvent);

				CurrentCommand = null;

				DepositTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		private void OnDepositDepleted (EntityDeathEvent _event) {
			bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);

			bus.Global(newEvent);

			CurrentCommand = null;
		}

		private void HarvestCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IHarvestable> deserialized && _event.CommandCancelled) {
				bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnDepositDepleted);

				HarvestTarget.Set(null, null);
				DepositTarget.Set(null, null);
			}
		}

		private void DepositCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IDepositable> deserialized && _event.CommandCancelled) {
				bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

				DepositTarget.Set(null, null);
				HarvestTarget.Set(null, null);
			}
		}

		/*	Interactions	*/

		private void FireRepair () {
			RepairTarget.Get.Attack(-repairAmount);
			currentRepairCooldown += repairCooldown;
		}

		private void SiphonOil () {
			int harvested = HarvestTarget.Get.Harvest("oil", this, harvestAmount, squad.storageComp.Submit);
			bus.Global(new ResourceHarvestedEvent(bus, HarvestTarget.Get, this, ResourceHarvestedEvent.Side.Harvester, harvested, "oil", squad.Stored, squad.Capacity));

			currentHarvestCooldown += harvestCooldown;
		}

		private void DepositResources () {
			squad.storageComp.Consume(DepositTarget.Get.Deposit("oil", depositAmount));
			bus.Global(new HarvesterDepositEvent(bus, this, HarvesterDepositEvent.Side.Harvester, squad.Stored, squad.Capacity, DepositTarget.Get));
			currentDepositCooldown += depositCooldown;
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