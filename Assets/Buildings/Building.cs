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
using MarsTS.UI;
using MarsTS.Vision;
using System.Linq;
using MarsTS.Research;

namespace MarsTS.Buildings {

	public abstract class Building : MonoBehaviour, ISelectable, ITaggable<Building>, IAttackable, ICommandable {

		public GameObject GameObject { get {  return gameObject;  } }

		/*	IAttackable Properties	*/

		public virtual int Health { get; protected set; }

		public virtual int MaxHealth { get { return maxHealth; } }

		[SerializeField]
		protected int maxHealth;

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

		public Commandlet CurrentCommand { get { return production.Current; } }

		public Commandlet[] CommandQueue { get { return production.Queue; } }

		public List<string> Active { 
			get { 
				return commands != null ? commands.Active : new(); 
			} 
		}

		public List<Timer> Cooldowns { 
			get { 
				return commands != null ? commands.Cooldowns : new(); 
			} 
		}

		public int Count { get { return production.Count; } }

		protected CommandQueue commands;

		protected ProductionQueue production;

		[SerializeField]
		protected string[] boundCommands;

		/*	Building Fields	*/

		[Header("Construction")]

		[SerializeField]
		protected int constructionWork;

		[SerializeField]
		protected int currentWork;

		[SerializeField]
		private GameObject ghost;

		[SerializeField]
		private CostEntry[] constructionCost;

		public int ConstructionProgress { get { return currentWork; } }

		public int ConstructionRequired { get { return constructionWork; } }

		public bool Constructed { get { return currentWork >= constructionWork; } }

		public GameObject SelectionGhost { get { return ghost; } }

		public CostEntry[] ConstructionCost { get { return constructionCost; } }

		protected int healthPerConstructionPoint;

		/*	Upgrades	*/

		public string RegistryType => "building";

		protected Entity entityComponent;

		protected EventAgent bus;

		protected Transform model;

		[SerializeField]
		protected GameObject[] visionObjects;

		protected virtual void Awake () {
			Health = 0;
			bus = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();
			commands = GetComponent<CommandQueue>();
			production = GetComponent<ProductionQueue>();

			healthPerConstructionPoint = Mathf.RoundToInt((float)MaxHealth / constructionWork);

			model = transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
				Health = maxHealth * (int) (currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;
		}

		protected virtual void Start () {
			//selectionCircle.GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();

			bus.AddListener<CommandStartEvent>(ExecuteOrder);
			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
			EventBus.AddListener<ResearchCompleteEvent>(OnGlobalResearchComplete) ;
		}

		protected virtual void CancelConstruction () {
			if (!Constructed) {
				bus.Global(new UnitDeathEvent(bus, this));

				foreach (CostEntry materialCost in ConstructionCost) {
					Player.Main.Resource(materialCost.key).Deposit(materialCost.amount);
				}

				Destroy(gameObject, 0.1f);
			}
		}

		protected virtual void Upgrade (Commandlet order) {
			bus.AddListener<CommandCompleteEvent>(UpgradeComplete);
		}

		protected virtual void UpgradeComplete (CommandCompleteEvent _event) {
			bus.RemoveListener<CommandCompleteEvent>(UpgradeComplete);

			IProducable order = _event.Command as IProducable;
			GameObject product = Instantiate(order.Product, transform, false);

			for (int i = 0; i < boundCommands.Length; i++) {
				if (boundCommands[i] == _event.Command.Command.Name) {
					boundCommands[i] = "";
					bus.Global(new CommandsUpdatedEvent(bus, this, Commands()));
					break;
				}
			}

			bus.Global(new ProductionCompleteEvent(bus, product, this, production, order));
		}

		protected virtual void Research (Commandlet order) {
			bus.AddListener<CommandCompleteEvent>(ResearchComplete);
		}

		protected virtual void ResearchComplete (CommandCompleteEvent _event) {
			bus.RemoveListener<CommandCompleteEvent>(ResearchComplete);

			IProducable order = _event.Command as IProducable;
			Technology product = Instantiate(order.Product, owner.transform, false).GetComponent<Technology>();
			Player.SubmitResearch(product);

			for (int i = 0; i < boundCommands.Length; i++) {
				if (boundCommands[i] == _event.Command.Command.Name) {
					boundCommands[i] = "";
					bus.Global(new CommandsUpdatedEvent(bus, this, Commands()));
					break;
				}
			}

			bus.Global(new ResearchCompleteEvent(bus, product, this, production, order));
			bus.Global(new ProductionCompleteEvent(bus, product.gameObject, this, production, order));
		}

		protected virtual void OnGlobalResearchComplete (ResearchCompleteEvent _event) {
			for (int i = 0; i < boundCommands.Length; i++) {
				if (_event.CurrentProduction.Get().Command.Name == boundCommands[i]) boundCommands[i] = "";
			}
		}

		public string[] Commands () {
			if (!Constructed) {
				return new string[] { "cancelConstruction" };
			}
			else return boundCommands;
		}

		public virtual void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "upgrade":
					production.Enqueue(order);
					return;
				case "cancelConstruction":
					CancelConstruction();
					return;
				case "research":
					production.Enqueue(order);
					return;
				default:
					return;
			}
		}

		protected virtual void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "upgrade":
					Upgrade(_event.Command);
					break;
				case "research":
					Research(_event.Command);
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

		public virtual void Select (bool status) {
			//selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
		}

		public virtual void Hover (bool status) {
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

		public virtual void Attack (int damage) {
			if (currentWork < constructionWork && damage < 0) {
				currentWork -= damage;

				float progress = (float)currentWork / constructionWork;

				Health += healthPerConstructionPoint;

				Health = Mathf.Clamp(Health, 0, MaxHealth);

				model.localScale = Vector3.one * progress;

				bus.Global(new UnitHurtEvent(bus, this));

				if (progress >= 1f) bus.Global(new CommandsUpdatedEvent(bus, this, boundCommands));
				return;
			}

			if (Health <= 0) return;
			if (damage < 0 && Health >= maxHealth) return;
			Health -= damage;
			bus.Global(new UnitHurtEvent(bus, this));

			if (Health <= 0) {
				bus.Global(new UnitDeathEvent(bus, this));
				Destroy(gameObject, 0.1f);
			}
		}

		public Command Evaluate (ISelectable target) {
			return CommandRegistry.Get("move");
		}

		public Commandlet Auto (ISelectable target) {
			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		protected virtual void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				HealthInfo info = _event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;

				_event.Info.Module<ProductionInfo>("productionQueue").SetQueue(this, production.Current as IProducable, production.QueuedProduction);
			}
		}

		protected virtual void OnVisionUpdate (EntityVisibleEvent _event) {
			bool visible = _event.Visible | GameVision.WasVisited(gameObject);

			foreach (GameObject hideable in visionObjects) {
				hideable.SetActive(visible);
			}
		}

		public bool CanCommand (string key) {
			bool canUse = true;

			if (!Constructed && key == "cancelConstruction") return true;

			for (int i = 0; i < boundCommands.Length; i++) {
				if (boundCommands[i] == key) break;

				if (i >= boundCommands.Length - 1) return false;
			}

			if (!commands.CanCommand(key)) canUse = false;
			if (!production.CanCommand(key)) canUse = false;

			return canUse;
		}
	}
}