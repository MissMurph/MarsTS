using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Harvesting;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units.SafeReference;
using Ratworx.MarsTS.Units.Sensors;
using Ratworx.MarsTS.Units.Turrets;
using Ratworx.MarsTS.WorldObject;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Infantry {

    public class Roughneck : InfantryMember {

		/*	Attacking	*/

		protected UnitReference<IAttackable> AttackTarget = new UnitReference<IAttackable>();

		private ProjectileTurret equippedWeapon;

		/*	Repairing	*/

		//How many units per second
		[SerializeField]
		private float repairRate;

		private int repairAmount;
		protected float repairCooldown;
		protected float currentRepairCooldown;

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

		/*	Infantry Fields	*/

		[SerializeField]
		private float sneakSpeed;
		private bool isSneaking;

		protected RoughneckSquad roughneckSquad;

		protected override void Awake () {
			base.Awake();

			equippedWeapon = GetComponentInChildren<ProjectileTurret>();
			repairSensor = transform.Find("InteractRange").GetComponent<AttackableSensor>();
			harvestableDetector = repairSensor.GetComponent<HarvestSensor>();
			depositableDetector = repairSensor.GetComponent<DepositSensor>();

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

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			_entityComponent.OnEntityInit += OnEntityInit;
			
			if (!NetworkManager.Singleton.IsServer) return;
		}

		private void OnEntityInit (Phase phase) {
			if (phase == Phase.Post
			    || _squad == null) return;

			roughneckSquad = _squad as RoughneckSquad;
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
					_currentPath = Path.Empty;
				}
				else if (!ReferenceEquals(TrackedTarget, AttackTarget.GameObject.transform)) {
					SetTarget(AttackTarget.GameObject.transform);
				}
			}

			if (RepairTarget.Get != null) {
				if (repairSensor.IsDetected(RepairTarget.Get)) {
					TrackedTarget = null;
					_currentPath = Path.Empty;

					if (currentRepairCooldown <= 0f) FireRepair();
				}
				else if (!ReferenceEquals(TrackedTarget, RepairTarget.GameObject.transform)) {
					SetTarget(RepairTarget.GameObject.transform);
				}
			}

			if (DepositTarget.Get != null) {
				if (depositableDetector.IsDetected(DepositTarget.Get)) {
					TrackedTarget = null;
					_currentPath = Path.Empty;

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
					_currentPath = Path.Empty;

					if (currentHarvestCooldown <= 0f) SiphonOil();

					currentHarvestCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, HarvestTarget.GameObject.transform)) {
					SetTarget(HarvestTarget.GameObject.transform);
				}
			}
		}

		public override void Order (Commandlet order, bool inclusive) {
			switch (order.Name) {
				case "sneak":
					Sneak(order);
					break;
				case "attack":
					Attack(order);
					break;
				case "repair":
					Repair(order);
					break;
				case "harvest":
					Harvest(order);
					break;
				case "deposit":
					Deposit(order);
					break;
				default:
					base.Order(order, inclusive);
					break;
			}
		}

		/*	Commands	*/

		/*	Attack	*/
		protected void Attack (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				AttackTarget.Set(deserialized.Target, deserialized.Target.GameObject);

				EntityCache.TryGetEntityComponent(AttackTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

				order.Callback.AddListener(AttackCancelled);
			}
		}

		//Could potentially move these to the actual Command Classes
		private void AttackCancelled (CommandCompleteEvent _event) {
			//bus.RemoveListener<CommandCompleteEvent>(AttackCancelled);

			if (_event.Command is Commandlet<IAttackable> deserialized && _event.IsCancelled) {
				EntityCache.TryGetEntityComponent(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				AttackTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		private void OnTargetDeath (UnitDeathEvent _event) {
			if (CurrentCommand == null) return;

			EntityCache.TryGetEntityComponent(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

			targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(_bus, CurrentCommand, true, this);

			CurrentCommand.Callback.Invoke(newEvent);

			Stop();
		}

		/*	Sneak	*/
		private void Sneak (Commandlet order) {
			Commandlet<bool> deserialized = order as Commandlet<bool>;
			if (deserialized.Target) {
				isSneaking = true;
				_currentSpeed = sneakSpeed;
			}
			else {
				isSneaking = false;
				_currentSpeed = _moveSpeed;
			}

			_bus.Local(new SneakEvent(_bus, this, isSneaking));

			PostSneakEventClientRpc(isSneaking);
		}

		[Rpc(SendTo.NotServer)]
		private void PostSneakEventClientRpc(bool status) => _bus.Local(new SneakEvent(_bus, this, status));

		/*	Repair	*/
		protected void Repair (Commandlet order) {
			if (order is Commandlet<IAttackable> deserialized) {
				IAttackable unit = deserialized.Target;

				if (unit.GetRelationship(_owner) == Relationship.Owned || unit.GetRelationship(_owner) == Relationship.Friendly) {
					RepairTarget.Set(unit, unit.GameObject);

					EntityCache.TryGetEntityComponent(RepairTarget.GameObject.transform.root.name, out EventAgent targetBus);

					targetBus.AddListener<UnitHurtEvent>(OnTargetHealed);
					targetBus.AddListener<UnitDeathEvent>(OnTargetDeath);

					order.Callback.AddListener(RepairCancelled);
				}
			}
		}

		private void RepairCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IAttackable> deserialized) {
				EntityCache.TryGetEntityComponent(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);
				targetBus.RemoveListener<UnitDeathEvent>(OnTargetDeath);

				RepairTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		private void OnTargetHealed (UnitHurtEvent _event) {
			if (CurrentCommand == null) return;

			if (_event.Targetable.Health >= _event.Targetable.MaxHealth) {
				EntityCache.TryGetEntityComponent(_event.Targetable.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(_bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);
			}
		}

		/*	Harvest	*/
		private void Harvest (Commandlet order) {
			if (order is Commandlet<IHarvestable> deserialized) {
				HarvestTarget.Set(deserialized.Target, deserialized.Target.GameObject);

				_bus.AddListener<ResourceHarvestedEvent>(OnExtraction);

				EntityCache.TryGetEntityComponent(HarvestTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<UnitDeathEvent>(OnDepositDepleted);

				order.Callback.AddListener(HarvestCancelled);
			}
		}

		private void OnExtraction (ResourceHarvestedEvent _event) {
			if (CurrentCommand == null) return;

			if (roughneckSquad.Stored >= roughneckSquad.Capacity) {
				_bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

				EntityCache.TryGetEntityComponent(_event.Unit.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnDepositDepleted);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(_bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);
			}
		}

		private void OnDepositDepleted (UnitDeathEvent _event) {
			_bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(_bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);
		}

		private void HarvestCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IHarvestable> deserialized) {
				_bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

				EntityCache.TryGetEntityComponent(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitDeathEvent>(OnDepositDepleted);

				HarvestTarget.Set(null, null);
				DepositTarget.Set(null, null);
			}
		}

		/*	Deposit	*/
		private void Deposit (Commandlet order) {
			if (order is Commandlet<IDepositable> deserialized) {
				DepositTarget.Set(deserialized.Target, deserialized.Target.GameObject);
				TrackedTarget = deserialized.Target.GameObject.transform;

				_bus.AddListener<HarvesterDepositEvent>(OnDeposit);

				order.Callback.AddListener(DepositCancelled);
			}
		}

		private void DepositCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IDepositable> deserialized && _event.IsCancelled) {
				_bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

				DepositTarget.Set(null, null);
				HarvestTarget.Set(null, null);
			}
		}

		private void OnDeposit (HarvesterDepositEvent _event) {
			if (CurrentCommand == null) return;

			if (roughneckSquad.Stored <= 0) {
				_bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(_bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				DepositTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		/*	Interactions	*/

		private void FireRepair () {
			RepairTarget.Get.Attack(-repairAmount);
			currentRepairCooldown += repairCooldown;
		}

		private void SiphonOil () {
			int harvested = HarvestTarget.Get.Harvest("oil", this, harvestAmount, roughneckSquad.storageComp.Submit);
			
			_bus.Global(
				new ResourceHarvestedEvent(
					_bus, 
					this, 
					ResourceHarvestedEvent.Side.Harvester,
					harvested,
					"oil",
					roughneckSquad.Stored,
					roughneckSquad.Capacity
				)
			);

			currentHarvestCooldown += harvestCooldown;
		}

		private void DepositResources () {
			roughneckSquad.storageComp.Consume(DepositTarget.Get.Deposit("oil", depositAmount));
			_bus.Global(new HarvesterDepositEvent(_bus, this, HarvesterDepositEvent.Side.Harvester, roughneckSquad.Stored, roughneckSquad.Capacity, DepositTarget.Get));
			currentDepositCooldown += depositCooldown;
		}
	}
}