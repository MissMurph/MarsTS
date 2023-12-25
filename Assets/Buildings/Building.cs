using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;
using MarsTS.UI;
using MarsTS.Vision;

namespace MarsTS.Buildings {

	public abstract class Building : MonoBehaviour, ISelectable, ITaggable<Building>, IRegistryObject<Building>, IAttackable, ICommandable {

		public GameObject GameObject { get {  return gameObject;  } }

		/*	IAttackable Properties	*/

		public int Health { get; protected set; }

		public int MaxHealth { get { return maxHealth; } }

		[SerializeField]
		private int maxHealth;

		/*	ISelectable Properties	*/

		public int ID { get { return entityComponent.ID; } }

		public string UnitType { get { return type; } }

		public string RegistryKey { get { return RegistryType + ":" + UnitType; } }

		public Sprite Icon { get { return icon; } }

		public Faction Owner { get { return owner; } }

		[SerializeField]
		private Sprite icon;

		[SerializeField]
		private string type;

		[SerializeField]
		protected Faction owner;

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(Building); } }

		/*	ICommandable Properties	*/

		public Commandlet CurrentCommand { get; protected set; }

		public Commandlet[] CommandQueue { get { return commandQueue.ToArray(); } }

		protected Queue<Commandlet> commandQueue = new Queue<Commandlet>();

		[SerializeField]
		private string[] boundCommands;

		/*	Building Fields	*/

		private GameObject selectionCircle;

		[Header("Construction")]

		[SerializeField]
		private int constructionWork;

		[SerializeField]
		private int currentWork;

		[SerializeField]
		private GameObject ghost;

		[SerializeField]
		private CostEntry[] constructionCost;

		public int ConstructionProgress { get { return currentWork; } }

		public int ConstructionRequired { get { return constructionWork; } }

		public bool Constructed { get { return currentWork >= constructionWork; } }

		public GameObject SelectionGhost { get { return ghost; } }

		public CostEntry[] ConstructionCost { get { return constructionCost; } }

		/*	Upgrades	*/

		//How many steps will occur per second
		[SerializeField]
		protected float productionSpeed;
		protected float stepTime;
		protected float timeToStep;

		public string RegistryType => "building";

		private Entity entityComponent;

		protected EventAgent bus;

		protected Transform model;

		[SerializeField]
		private GameObject[] visionObjects;

		protected virtual void Awake () {
			selectionCircle = transform.Find("SelectionCircle").gameObject;
			selectionCircle.SetActive(false);
			Health = 1;

			bus = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();

			model = transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
				Health = maxHealth * (int) (currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;

			stepTime = 1f / productionSpeed;
			timeToStep = stepTime;
		}

		protected virtual void Start () {
			selectionCircle.GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();

			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
			EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
			EventBus.AddListener<VisionInitEvent>(OnVisionInit);
		}

		protected virtual void Update () {
			UpdateQueue();

			if (CurrentCommand != null && CurrentCommand is ProductionCommandlet production && production.Name == "upgrade") {
				timeToStep -= Time.deltaTime;

				if (timeToStep <= 0) {
					production.ProductionProgress++;

					if (production.ProductionProgress >= production.ProductionRequired) {
						Upgrade(production);
					}
					else bus.Global(ProductionEvent.Step(bus, this));

					timeToStep += stepTime;
				}
			}
		}

		protected void UpdateQueue () {
			if (CurrentCommand is null && commandQueue.TryDequeue(out Commandlet order)) {

				CurrentCommand = order;

				bus.Global(ProductionEvent.Started(bus, this));
			}
		}

		private void OnVisionInit (VisionInitEvent _event) {
			bool visible = GameVision.IsVisible(gameObject, Player.Main.VisionMask);

			foreach (GameObject hideable in visionObjects) {
				hideable.SetActive(visible);
			}
		}

		protected virtual void CancelConstruction () {
			if (!Constructed) {
				bus.Global(new EntityDeathEvent(bus, this));

				foreach (CostEntry materialCost in ConstructionCost) {
					Player.Main.Resource(materialCost.key).Deposit(materialCost.amount);
				}

				Destroy(gameObject, 0.1f);
			}
		}

		protected virtual void Upgrade (Commandlet order) {
			ProductionCommandlet production = order as ProductionCommandlet;
			Instantiate(production.Target, transform, false);
			CurrentCommand = null;
			bus.Global(new ProductionCompleteEvent(bus, production.Target, this));
		}

		public string[] Commands () {
			if (!Constructed) {
				return new string[] { "cancelConstruction" };
			}
			else return boundCommands;
		}

		public virtual void Enqueue (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			Execute(order);
		}

		public virtual void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			if (order.Name == "upgrade") {
				commandQueue.Enqueue(order);
				bus.Global(ProductionEvent.Queued(bus, this));
				return;
			}

			ProcessOrder(order);
		}

		protected virtual void ProcessOrder (Commandlet order) {
			switch (order.Name) {
				case "cancelConstruction":
					CancelConstruction();
					break;
				default:
				break;
			}
		}

		public Building Get () {
			return this;
		}

		public Relationship GetRelationship (Faction other) {
			Relationship result = owner.GetRelationship(other);
			return result;
		}

		public void Select (bool status) {
			//selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
		}

		public void Hover (bool status) {
			//These are seperated due to the Player Selection Check
			if (status) {
				//selectionCircle.SetActive(true);
				bus.Local(new UnitHoverEvent(bus, status));
			}
			else if (!Player.Main.HasSelected(this)) {
				//selectionCircle.SetActive(false);
				bus.Local(new UnitHoverEvent(bus, status));
			}
		}

		public bool SetOwner (Faction player) {
			owner = player;
			return true;
		}

		public void Attack (int damage) {
			if (currentWork < constructionWork && damage < 0) {
				currentWork -= damage;
				float progress = (float)currentWork / constructionWork;
				Health = (int)(maxHealth * progress);

				model.localScale = Vector3.one * progress;

				bus.Global(new UnitHurtEvent(bus, this));

				if (progress >= 1f) bus.Global(new CommandsUpdatedEvent(bus, this, boundCommands));
				return;
			}

			if (damage < 0 && Health >= maxHealth) return;
			Health -= damage;
			bus.Global(new UnitHurtEvent(bus, this));

			if (Health <= 0) {
				bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject, 0.1f);
			}
		}

		public Command Evaluate (ISelectable target) {
			return CommandRegistry.Get("move");
		}

		public IRegistryObject<Building> GetRegistryEntry () {
			throw new NotImplementedException();
		}

		public Commandlet Auto (ISelectable target) {
			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		protected virtual void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				HealthInfo info = _event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;

				if (commandQueue.Count > 0) {
					_event.Info.Module<ProductionQueue>("productionQueue").SetQueue(this, CurrentCommand as ProductionCommandlet, commandQueue.ToArray() as ProductionCommandlet[]);
				}
			}
		}

		protected virtual void OnVisionUpdate (VisionUpdateEvent _event) {
			bool visible = GameVision.IsVisible(gameObject, Player.Main.VisionMask);

			if (visible) {
				foreach (GameObject hideable in visionObjects) {
					hideable.SetActive(visible);
				}
			}

			//if (GameVision.WasVisited(gameObject, Player.Main.VisionMask)) bus.Global(new UnitVisibleEvent(bus, this, true));
		}
	}
}