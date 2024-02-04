using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;
using UnityEngine.SocialPlatforms.Impl;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.PackageManager;
using UnityEngine.ProBuilder;

namespace MarsTS.Units {

	public class InfantrySquad : MonoBehaviour, ISelectable, ITaggable<InfantrySquad>, ICommandable, IAttackable {
		public GameObject GameObject { get { return gameObject; } }

		/*	ISelectable Properties	*/

		public int ID { get { return entityComponent.ID; } }

		public string UnitType { get { return type; } }

		public string RegistryKey { get { return "unit:" + UnitType; } }

		public Faction Owner { get { return owner; } }

		public Sprite Icon { get { return icon; } }

		[SerializeField]
		private Sprite icon;

		[SerializeField]
		private string type;

		[SerializeField]
		private Faction owner;

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(InfantrySquad); } }

		/*	ICommandable Properties	*/

		public Commandlet CurrentCommand { get { return commands.Current; } }

		public Commandlet[] CommandQueue { get { return commands.Queue; } }

		public List<string> Active { get { return commands.Active; } }

		public List<Cooldown> Cooldowns { get { return commands.Cooldowns; } }

		[SerializeField]
		private string[] boundCommands;

		protected CommandQueue commands;

		/*	InfantrySquad Fields	*/

		private List<InfantryMember> members = new List<InfantryMember>();
		private List<Transform> selectionColliders = new List<Transform>();

		private Entity entityComponent;

		[SerializeField]
		private int maxMembers;

		[SerializeField]
		private InfantryMember[] startingMembers;

		[SerializeField]
		private GameObject selectionColliderPrefab;

		private EventAgent bus;

		public int Stored { get { return storageComp.Amount; } }

		public int Capacity { get { return storageComp.Capacity; } }

		public int Health {
			get {
				int current = 0;

				foreach (InfantryMember member in members) {
					current += member.Health;
				}

				return current;
			}
		}

		public int MaxHealth { get { return members.Count * members[0].MaxHealth; } }

		public ResourceStorage storageComp;

		private Transform resourceBar;

		private void Awake () {
			foreach (InfantryMember unit in startingMembers) {
				unit.SetOwner(owner);
				unit.squad = this;

				Transform newSelectionCollider = Instantiate(selectionColliderPrefab, transform).transform;
				newSelectionCollider.position = unit.transform.position;
				selectionColliders.Add(newSelectionCollider);

				members.Add(unit);

				EventAgent unitEvents = unit.GetComponent<EventAgent>();
				unitEvents.AddListener<EntityDeathEvent>(OnMemberDeath);
				unitEvents.AddListener<UnitHurtEvent>(OnMemberHurt);
				unitEvents.AddListener<ResourceHarvestedEvent>(OnMemberHarvest);
				unitEvents.AddListener<HarvesterDepositEvent>(OnMemberDeposit);
			}

			entityComponent = GetComponent<Entity>();
			bus = GetComponent<EventAgent>();
			storageComp = GetComponent<ResourceStorage>();
			commands = GetComponent<CommandQueue>();
			resourceBar = transform.Find("BarOrientation");
		}

		private void Start () {
			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
			bus.AddListener<EntityInitEvent>(OnEntityInit);
			bus.AddListener<CommandStartEvent>(ExecuteOrder);
		}

		private void OnEntityInit (EntityInitEvent _event) {
			
		}

		private void Update () {
			//if (!initialized) return;

			resourceBar.transform.position = members[0].transform.position;

			for (int i = 0; i < members.Count; i++) {
				selectionColliders[i].transform.position = members[i].transform.position;
			}
		}

		private void OnMemberHurt (UnitHurtEvent _event) {
			bus.Global(new UnitHurtEvent(bus, this));
		}

		public void OnMemberHarvest (ResourceHarvestedEvent _event) {
			bus.Global(new ResourceHarvestedEvent(bus, _event.Deposit, this, ResourceHarvestedEvent.Side.Harvester, _event.HarvestAmount, _event.Resource, Stored, Capacity));
		}

		public void OnMemberDeposit (HarvesterDepositEvent _event) {
			bus.Global(new HarvesterDepositEvent(bus, this, HarvesterDepositEvent.Side.Harvester, Stored, Capacity, _event.Bank));
		}

		private void OnMemberDeath (EntityDeathEvent _event) {
			int currentMemberCount = members.Count;

			Transform lastCollider = selectionColliders[currentMemberCount - 1];

			selectionColliders.Remove(lastCollider);
			members.Remove(_event.Unit as InfantryMember);

			Destroy(lastCollider.gameObject);

			if (members.Count <= 0) {
				bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject);
			}
		}

		protected virtual void ExecuteOrder (CommandStartEvent _event) {
			/*switch (order.Name) {
				case "move":
				//CurrentCommand = order;
				DistributeOrder(order);
				break;
				case "stop":
				//CurrentCommand = order;
				DistributeOrder(order);
				break;
				default:
				break;
			}*/

			foreach (InfantryMember unit in members) {
				unit.Order(_event.Command, false);
			}
		}

		public string[] Commands () {
			return boundCommands;
		}

		public void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
				case "sneak":
					SquadSneak(order);
					return;
				case "attack":
					
					break;
				case "repair":
					
					break;
				case "harvest":
					
					break;
				case "deposit":
					
					break;
				case "move":

					break;
				case "stop":
					
					break;
				default:
					return;
			}
			if (inclusive) commands.Enqueue(order);
			else commands.Execute(order);
		}

		private void SquadSneak (Commandlet order) {
			if (!CanCommand(order.Name)) return;
			Commandlet<bool> deserialized = order as Commandlet<bool>;

			commands.Activate(order, deserialized.Target);

			foreach (InfantryMember unit in members) {
				unit.Order(order, false);
			}
		}

		public Command Evaluate (ISelectable target) {
			if (target is IHarvestable harvestable
				&& Stored < Capacity
				&& harvestable.StoredAmount > 0
				&& harvestable.CanHarvest(storageComp.Resource, this)) {
				return CommandRegistry.Get("harvest");
			}

			if (target is IDepositable
				&& Stored > 0) {
				return CommandRegistry.Get("deposit");
			}

			if (target is IAttackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get("attack");
			}

			return CommandRegistry.Get("move");
		}

		public Commandlet Auto (ISelectable target) {
			if (target is IHarvestable harvestable
				&& Stored < Capacity
				&& harvestable.StoredAmount > 0
				&& harvestable.CanHarvest(storageComp.Resource, this)) {
				return CommandRegistry.Get<Harvest>("harvest").Construct(harvestable);
			}

			if (target is IDepositable depositable
				&& Stored > 0) {
				return CommandRegistry.Get<Deposit>("deposit").Construct(depositable);
			}

			if (target is IAttackable attackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get<Attack>("attack").Construct(attackable);
			}

			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		public InfantrySquad Get () {
			return this;
		}

		public Relationship GetRelationship (Faction player) {
			return Owner.GetRelationship(player);
		}

		public void Hover (bool status) {
			foreach (InfantryMember unit in members) {
				unit.Hover(status);
			}
		}

		public void Select (bool status) {
			foreach (InfantryMember unit in members) {
				unit.Select(status);
			}
		}

		public bool SetOwner (Faction player) {
			owner = player;
			return true;
		}

		protected virtual void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				HealthInfo info = _event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;

				StorageInfo storage = _event.Info.Module<StorageInfo>("storage");
				storage.CurrentUnit = this;
				storage.CurrentValue = Stored;
				storage.MaxValue = Capacity;
			}
		}

		public void Attack (int damage) {
			throw new NotImplementedException();
		}

		public bool CanCommand (string key) {
			bool canUse = false;

			for (int i = 0; i < boundCommands.Length; i++) {
				if (boundCommands[i] == key) break;

				if (i >= boundCommands.Length - 1) return false;
			}

			if (commands.CanCommand(key)) canUse = true;
			//if (production.CanCommand(key)) canUse = true;

			return canUse;
		}
	}
}