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

namespace MarsTS.Buildings {

	public abstract class Building : MonoBehaviour, ISelectable, ITaggable<Building>, IAttackable, ICommandable {

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

		public Commandlet CurrentCommand { get { return commands.Current; } }

		public Commandlet[] CommandQueue { get { return commands.Queue; } }

		public List<string> Active { get { return commands.Active; } }

		public List<Cooldown> Cooldowns { get { return commands.Cooldowns; } }

		protected CommandQueue commands;

		protected ProductionQueue production;

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
			commands = GetComponent<CommandQueue>();
			production = GetComponent<ProductionQueue>();

			bus = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();

			model = transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
				Health = maxHealth * (int) (currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;
		}

		protected virtual void Start () {
			selectionCircle.GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();

			bus.AddListener<CommandStartEvent>(ExecuteOrder);

			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
			EventBus.AddListener<VisionUpdateEvent>(OnVisionUpdate);
			EventBus.AddListener<VisionInitEvent>(OnVisionInit);
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
				default:
					return;
			}
		}

		protected virtual void ExecuteOrder (CommandStartEvent _event) {
			switch (_event.Command.Name) {
				case "cancelConstruction":
					CancelConstruction();
					break;
				case "upgrade":
					Upgrade(_event.Command);
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

		protected virtual void OnVisionUpdate (VisionUpdateEvent _event) {
			bool visible = GameVision.IsVisible(gameObject, Player.Main.VisionMask);

			if (visible) {
				foreach (GameObject hideable in visionObjects) {
					hideable.SetActive(visible);
				}
			}

			//if (GameVision.WasVisited(gameObject, Player.Main.VisionMask)) bus.Global(new UnitVisibleEvent(bus, this, true));
		}

		public bool CanCommand (string key) {
			bool canUse = true;

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