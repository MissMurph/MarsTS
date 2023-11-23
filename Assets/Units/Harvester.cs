using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.CanvasScaler;

namespace MarsTS.Units {

	public class Harvester : Unit {

		[Header("Movement")]

		[SerializeField]
		private float topSpeed;

		[SerializeField]
		private float acceleration;

		[SerializeField]
		private float turnSpeed;

		private float CurrentAngle {
			get {
				float angle = transform.rotation.eulerAngles.y;
				return angle;
			}
		}

		[SerializeField]
		private float angleTolerance;

		private float velocity;

		[Header("Turrets")]

		protected Dictionary<string, HarvesterTurret> registeredTurrets = new Dictionary<string, HarvesterTurret>();

		[SerializeField]
		protected HarvesterTurret[] turretsToRegister;

		public IHarvestable HarvestTarget {
			get {
				return harvestTarget;
			}
			set {
				if (harvestTarget != null) {
					EntityCache.TryGet(harvestTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => HarvestTarget = null);
				}

				harvestTarget = value;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => HarvestTarget = null);
				}
			}
		}

		private IHarvestable harvestTarget;

		private IDepositable DepositTarget {
			get {
				return depositTarget;
			}
			set {
				if (depositTarget != null) {
					EntityCache.TryGet(depositTarget.GameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => DepositTarget = null);
				}

				depositTarget = value;

				if (value != null) {
					EntityCache.TryGet(value.GameObject.name + ":eventAgent", out EventAgent agent);
					agent.AddListener<EntityDeathEvent>((_event) => DepositTarget = null);
				}
			}
		}

		private IDepositable depositTarget;

		private int Stored { get { return storageComp.Amount; } }

		private int Capacity { get { return storageComp.Capacity; } }

		private ResourceStorage storageComp;

		private DepositSensor depoSensor;

		//This is how many units per second
		[SerializeField]
		private float depositRate;

		private int depositAmount;
		private float cooldown;
		private float currentCooldown;

		protected override void Awake () {
			base.Awake();

			storageComp = GetComponent<ResourceStorage>();
			depoSensor = GetComponentInChildren<DepositSensor>();

			cooldown = 1f / depositRate;
			depositAmount = Mathf.RoundToInt(depositRate * cooldown);
			currentCooldown = cooldown;

			foreach (HarvesterTurret turret in turretsToRegister) {
				registeredTurrets.TryAdd(turret.name, turret);
			}
		}

		protected override void Update () {
			base.Update();

			if (DepositTarget != null) {
				if (depoSensor.IsInRange(DepositTarget)) {
					TrackedTarget = null;
					currentPath = Path.Empty;

					if (currentCooldown <= 0f) DepositResources();

					currentCooldown -= Time.deltaTime;
				}
				else if (!ReferenceEquals(TrackedTarget, DepositTarget.GameObject.transform)) {
					SetTarget(DepositTarget.GameObject.transform);
				}

				return;
			}

			if (HarvestTarget != null) {
				if (registeredTurrets["turret_main"].IsInRange(HarvestTarget as ISelectable)) {
					TrackedTarget = null;
					currentPath = Path.Empty;
				}
				else if (!ReferenceEquals(TrackedTarget, HarvestTarget.GameObject.transform)) {
					SetTarget(HarvestTarget.GameObject.transform);
				}
			}
		}

		protected virtual void FixedUpdate () {
			velocity = body.velocity.magnitude;

			if (!currentPath.IsEmpty) {
				Vector3 targetWaypoint = currentPath[pathIndex];

				Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
				float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

				float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, turnSpeed * Time.fixedDeltaTime);
				body.MoveRotation(Quaternion.Euler(transform.rotation.x, newAngle, transform.rotation.z));

				Vector3 currentVelocity = body.velocity;
				Vector3 adjustedVelocity = transform.forward * currentVelocity.magnitude;

				if (Vector3.Angle(targetDirection, transform.forward) <= angleTolerance) {
					float accelCap = 1f - (velocity / topSpeed);

					body.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (turnSpeed * accelCap) * Time.fixedDeltaTime);

					//Relative so it can take into account the forward vector of the car
					body.AddRelativeForce(Vector3.forward * (acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);
				}
			}
			else if (velocity >= 0.5f) {
				body.AddRelativeForce(-body.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
			}
		}

		protected override void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "harvest":
					CurrentCommand = order;
					Harvest(order);
					break;
				case "deposit":
					CurrentCommand = order; 
					Deposit(order);
					break;
				default:
					base.ProcessOrder(order);
					break;
			}
		}

		private void Harvest (Commandlet order) {
			if (Stored >= Capacity) {
				FindDepositable();
			}

			if (order is Commandlet<IHarvestable> deserialized) {
				HarvestTarget = deserialized.Target;

				bus.AddListener<HarvesterExtractionEvent>(OnExtraction);

				EntityCache.TryGet(HarvestTarget.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.AddListener<EntityDeathEvent>(OnDepositDepleted);

				order.Callback.AddListener(HarvestCancelled);
			}
		}

		private void Deposit (Commandlet order) {
			if (order is Commandlet<IDepositable> deserialized) {
				DepositTarget = deserialized.Target;
				TrackedTarget = deserialized.Target.GameObject.transform;

				bus.AddListener<HarvesterDepositEvent>(OnDeposit);

				order.Callback.AddListener(DepositCancelled);
			}
		}

		private void DepositResources () {
			storageComp.Consume(DepositTarget.Deposit("resource_unit", depositAmount));
			bus.Global(new HarvesterDepositEvent(bus, this, Stored, Capacity, DepositTarget));
			currentCooldown += cooldown;
		}

		private void OnDeposit (HarvesterDepositEvent _event) {
			if (Stored <= 0) {
				bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				bus.Global(newEvent);

				CurrentCommand = null;

				DepositTarget = null;
				TrackedTarget = null;

				if (HarvestTarget != null) Enqueue(CommandRegistry.Get<Harvest>("harvest").Construct(HarvestTarget));
			}
		}

		private void FindDepositable () {
			IDepositable closestBank = null;
			float currentDist = 1000f;

			foreach (IDepositable bank in Player.Depositables) {
				float newDistance = Vector3.Distance(bank.GameObject.transform.position, transform.position);

				if (newDistance < currentDist) {
					closestBank = bank;
				}
			}

			if (closestBank != null) {
				Execute(CommandRegistry.Get<Deposit>("deposit").Construct(closestBank));
			}
		}

		//Could potentially move these to the actual Command Classes
		private void OnExtraction (HarvesterExtractionEvent _event) {
			if (Stored >= Capacity) {
				bus.RemoveListener<HarvesterExtractionEvent>(OnExtraction);

				EntityCache.TryGet(_event.Deposit.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnDepositDepleted);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				bus.Global(newEvent);

				CurrentCommand = null;

				FindDepositable();
			}
		}

		private void OnDepositDepleted (EntityDeathEvent _event) {
			bus.RemoveListener<HarvesterExtractionEvent>(OnExtraction);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);

			bus.Global(newEvent);

			CurrentCommand = null;
		}

		private void HarvestCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IHarvestable> deserialized && _event.CommandCancelled) {
				bus.RemoveListener<HarvesterExtractionEvent>(OnExtraction);

				EntityCache.TryGet(deserialized.Target.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<EntityDeathEvent>(OnDepositDepleted);

				HarvestTarget = null;
				DepositTarget = null;
			}
		}

		private void DepositCancelled (CommandCompleteEvent _event) {
			if (_event.Command is Commandlet<IDepositable> deserialized && _event.CommandCancelled) {
				bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

				DepositTarget = null;
				HarvestTarget = null;
			}
		}

		public override Command Evaluate (ISelectable target) {
			if (target is IHarvestable) {
				return CommandRegistry.Get("harvest");
			}

			if (target is IDepositable && Stored > 0) {
				return CommandRegistry.Get("deposit");
			}

			return CommandRegistry.Get("move");
		}

		public override Commandlet Auto (ISelectable target) {
			if (target is IHarvestable harvestable
				&& Stored < Capacity
				&& harvestable.StoredAmount > 0) {
				return CommandRegistry.Get<Harvest>("harvest").Construct(harvestable);
			}

			if (target is IDepositable deserialized
				&& Stored > 0) {
				return CommandRegistry.Get<Deposit>("deposit").Construct(deserialized);
			}

			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}
	}
}