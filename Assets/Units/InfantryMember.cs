using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Vision;
using MarsTS.World;
using MarsTS.World.Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

	public class InfantryMember : MonoBehaviour, ISelectable, ITaggable<InfantryMember>, IAttackable, ICommandable {

		public GameObject GameObject { get { return gameObject; } }

		/*	IAttackable Properties	*/

		public int Health { get { return currentHealth; } }

		public int MaxHealth { get { return maxHealth; } }

		[SerializeField]
		protected int maxHealth;

		[SerializeField]
		protected int currentHealth;

		/*	ISelectable Properties	*/

		public int ID { get { return entityComponent.ID; } }

		public string UnitType { get { return type; } }

		public string RegistryKey { get { return "unit:" + UnitType; } }

		public Sprite Icon { get { return icon; } }

		public Faction Owner { get { return squad.Owner; } }

		[SerializeField]
		private Sprite icon;

		[SerializeField]
		private string type;

		[SerializeField]
		protected Faction owner;

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(Unit); } }

		/*	ICommandable Properties	*/

		public Commandlet CurrentCommand { get { return squad.CurrentCommand; } }

		public Commandlet[] CommandQueue { get { return squad.CommandQueue; } }

		public List<string> Active => throw new NotImplementedException();

		public List<Cooldown> Cooldowns => throw new NotImplementedException();

		public int Count => throw new NotImplementedException();

		/*	Infantry Fields	*/

		protected Entity entityComponent;

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

		

		protected Transform TrackedTarget {
			get {
				return target;
			}
			set {
				if (target != null) {
					EntityCache.TryGet(target.gameObject.name + ":eventAgent", out EventAgent oldAgent);
					oldAgent.RemoveListener<EntityDeathEvent>((_event) => TrackedTarget = null);
				}

				target = value;

				if (value != null) {
					EntityCache.TryGet(value.gameObject.name + ":eventAgent", out EventAgent agent);

					agent.AddListener<EntityDeathEvent>((_event) => TrackedTarget = null);

					SetTarget(value);
				}
			}
		}

		private Transform target;

		private Vector3 targetOldPos;

		protected Path currentPath {
			get;
			set;
		} = Path.Empty;

		private float angle;
		protected int pathIndex;

		protected Rigidbody body;

		private const float minPathUpdateTime = .5f;
		private const float pathUpdateMoveThreshold = .5f;

		[SerializeField]
		protected float waypointCompletionDistance;

		protected EventAgent bus;

		[SerializeField]
		private GameObject[] hideables;

		/*	Attacking	*/

		protected UnitReference<IAttackable> AttackTarget = new UnitReference<IAttackable>();

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

		protected virtual void Awake () {
			transform.parent = null;

			body = GetComponent<Rigidbody>();
			entityComponent = GetComponent<Entity>();
			bus = GetComponent<EventAgent>();

			currentHealth = maxHealth;

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

		protected virtual void Start () {
			StartCoroutine(UpdatePath());

			transform.Find("SelectionCircle").GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();

			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

			EventBus.AddListener<VisionInitEvent>(OnVisionInit);
		}

		private void OnVisionInit (VisionInitEvent _event) {
			bool visible = GameVision.IsVisible(gameObject);

			foreach (GameObject hideable in hideables) {
				hideable.SetActive(visible);
			}
		}

		protected virtual void Update () {
			if (!currentPath.IsEmpty) {
				Vector3 targetWaypoint = currentPath[pathIndex];
				float distance = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).magnitude;

				if (distance <= waypointCompletionDistance) {
					pathIndex++;
				}

				if (pathIndex >= currentPath.Length) {
					bus.Local(new PathCompleteEvent(bus, true));
					currentPath = Path.Empty;
				}
			}

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

		protected IEnumerator UpdatePath () {
			if (Time.timeSinceLevelLoad < .5f) {
				yield return new WaitForSeconds(.5f);
			}

			float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;

			while (true) {
				yield return new WaitForSeconds(minPathUpdateTime);

				if (target != null && (target.position - targetOldPos).sqrMagnitude > sqrMoveThreshold) {
					PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
					targetOldPos = target.position;
				}
			}
		}

		public void OnDrawGizmos () {
			if (!currentPath.IsEmpty) {
				for (int i = pathIndex; i < currentPath.Length; i++) {
					Gizmos.color = Color.black;
					Gizmos.DrawCube(currentPath[i], Vector3.one / 2);

					if (i == pathIndex) {
						Gizmos.DrawLine(transform.position, currentPath[i]);
					}
					else {
						Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
					}
				}
			}
		}

		private void OnPathFound (Path newPath, bool pathSuccessful) {
			if (pathSuccessful) {
				currentPath = newPath;
				pathIndex = 0;
			}
		}

		protected void SetTarget (Vector3 _target) {
			PathRequestManager.RequestPath(transform.position, _target, OnPathFound);
		}

		protected void SetTarget (Transform _target) {
			SetTarget(_target.position);
			target = _target;
		}

		protected virtual void Stop () {
			currentPath = Path.Empty;
			target = null;

			//CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, false, this);
			//bus.Global(_event);

			//CurrentCommand = null;
		}

		public void Order (Commandlet order, bool inclusive) {
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
				case "move":
					Move(order);
					break;
				case "stop":
					Stop();
					break;
				default:
					
					break;
			}
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

		protected virtual void Move (Commandlet order) {
			if (order is Commandlet<Vector3> deserialized) {
				SetTarget(deserialized.Target);

				bus.AddListener<PathCompleteEvent>(OnPathComplete);
				order.Callback.AddListener((_event) => bus.RemoveListener<PathCompleteEvent>(OnPathComplete));
			}
		}

		private void OnPathComplete (PathCompleteEvent _event) {
			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);
		}

		private void Sneak (Commandlet order) {
			Commandlet<bool> deserialized = order as Commandlet<bool>;
			if (deserialized.Target) {
				isSneaking = true;
				currentSpeed = sneakSpeed;
			}
			else {
				isSneaking = false;
				currentSpeed = moveSpeed;
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

			Stop();
		}

		//Could potentially move these to the actual Command Classes
		private void OnTargetHealed (UnitHurtEvent _event) {
			if (_event.Targetable.Health >= _event.Targetable.MaxHealth) {
				EntityCache.TryGet(_event.Targetable.GameObject.transform.root.name, out EventAgent targetBus);

				targetBus.RemoveListener<UnitHurtEvent>(OnTargetHealed);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);
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
			}
		}

		private void OnDeposit (HarvesterDepositEvent _event) {
			if (squad.Stored <= 0) {
				bus.RemoveListener<HarvesterDepositEvent>(OnDeposit);

				CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

				CurrentCommand.Callback.Invoke(newEvent);

				DepositTarget.Set(null, null);
				TrackedTarget = null;
			}
		}

		private void OnDepositDepleted (EntityDeathEvent _event) {
			bus.RemoveListener<ResourceHarvestedEvent>(OnExtraction);

			CommandCompleteEvent newEvent = new CommandCompleteEvent(bus, CurrentCommand, false, this);

			CurrentCommand.Callback.Invoke(newEvent);
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

		public virtual void Select (bool status) {
			//selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
			isSelected = status;
		}

		public virtual void Hover (bool status) {
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

		public virtual Commandlet Auto (ISelectable target) {
			throw new System.NotImplementedException();
		}

		public virtual Command Evaluate (ISelectable target) {
			throw new System.NotImplementedException();
		}

		public Relationship GetRelationship (Faction other) {
			return Owner.GetRelationship(other);
		}

		public bool SetOwner (Faction player) {
			owner = player;
			return true;
		}

		public void Attack (int damage) {
			if (damage < 0 && currentHealth >= maxHealth) return;
			currentHealth -= damage;

			if (currentHealth <= 0) {
				bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject);
			}
			else bus.Global(new UnitHurtEvent(bus, this));
		}

		public InfantryMember Get () {
			return this;
		}

		public string[] Commands () {
			return new string[0];
		}

		protected virtual void OnVisionUpdate (EntityVisibleEvent _event) {
			foreach (GameObject hideable in hideables) {
				hideable.SetActive(_event.Visible);
			}
		}

		public bool CanCommand (string key) {
			throw new NotImplementedException();
		}
	}
}