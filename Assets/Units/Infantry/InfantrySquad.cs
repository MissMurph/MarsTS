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
using static UnityEngine.UI.CanvasScaler;

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
		protected Faction owner;

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(InfantrySquad); } }

		/*	ICommandable Properties	*/

		public Commandlet CurrentCommand { get { return commands.Current; } }

		public Commandlet[] CommandQueue { get { return commands.Queue; } }

		public List<string> Active { get { return commands.Active; } }

		public List<Timer> Cooldowns { get { return commands.Cooldowns; } }

		public int Count { get { return commands.Count; } }

		[SerializeField]
		protected string[] boundCommands;

		protected CommandQueue commands;

		/*	InfantrySquad Fields	*/

		public List<ISelectable> Members {
			get {
				List<ISelectable> output = new();

				foreach (MemberEntry unitEntry in members.Values) {
					output.Add(unitEntry.member);
				}

				return output;
			}
		}

		protected Dictionary<string, MemberEntry> members = new Dictionary<string, MemberEntry>();

		protected Entity entityComponent;

		[SerializeField]
		protected int maxMembers;

		[SerializeField]
		protected InfantryMember[] startingMembers;

		[SerializeField]
		protected GameObject selectionColliderPrefab;

		protected EventAgent bus;

		public int Health {
			get {
				int current = 0;

				foreach (MemberEntry unitEntry in members.Values) {
					current += unitEntry.member.Health;
				}

				return current;
			}
		}

		public int MaxHealth { 
			get {
				int max = 0;

				foreach (MemberEntry unitEntry in members.Values) {
					max += unitEntry.member.MaxHealth;
				}

				return max;
			} 
		}

		protected virtual void Awake () {
			entityComponent = GetComponent<Entity>();
			bus = GetComponent<EventAgent>();
			commands = GetComponent<CommandQueue>();

			foreach (InfantryMember unit in startingMembers) {
				RegisterMember(unit);
			}
		}

		protected virtual void Start () {
			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
			bus.AddListener<CommandStartEvent>(ExecuteOrder);
		}

		protected virtual void Update () {
			foreach (MemberEntry entry in members.Values) {
				entry.selectionCollider.transform.position = entry.member.transform.position;
			}
		}

		protected virtual void RegisterMember (InfantryMember unit) {
			unit.SetOwner(owner);
			unit.squad = this;

			EventAgent unitEvents = unit.GetComponent<EventAgent>();
			unitEvents.AddListener<EntityDeathEvent>(OnMemberDeath);
			unitEvents.AddListener<UnitHurtEvent>(OnMemberHurt);
			unitEvents.AddListener<EntityInitEvent>(OnMemberInit);
		}

		protected virtual void OnMemberInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Post) return;
			MemberEntry newEntry = new MemberEntry();

			Transform newSelectionCollider = Instantiate(selectionColliderPrefab, transform).transform;
			newSelectionCollider.position = _event.ParentEntity.gameObject.transform.position;

			newEntry.key = _event.ParentEntity.gameObject.name;
			newEntry.member = _event.ParentEntity.Get<InfantryMember>("selectable");
			newEntry.selectionCollider = newSelectionCollider;
			newEntry.bus = _event.Source;

			members[newEntry.key] = newEntry;
		}

		private void OnMemberHurt (UnitHurtEvent _event) {
			bus.Global(new UnitHurtEvent(bus, this));
		}

		private void OnMemberDeath (EntityDeathEvent _event) {
			MemberEntry deadEntry = members[_event.Unit.GameObject.name];

			members.Remove(deadEntry.key);

			Destroy(deadEntry.selectionCollider.gameObject);

			if (members.Count <= 0) {
				bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject);
			}
		}

		protected virtual void ExecuteOrder (CommandStartEvent _event) {
			foreach (MemberEntry entry in members.Values) {
				entry.member.Order(_event.Command, false);
			}
		}

		public virtual string[] Commands () {
			return boundCommands;
		}

		public  virtual void Order (Commandlet order, bool inclusive) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;

			switch (order.Name) {
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

		public virtual Command Evaluate (ISelectable target) {
			return CommandRegistry.Get("move");
		}

		public virtual Commandlet Auto (ISelectable target) {
			return CommandRegistry.Get<Move>("move").Construct(target.GameObject.transform.position);
		}

		public InfantrySquad Get () {
			return this;
		}

		public Relationship GetRelationship (Faction player) {
			return Owner.GetRelationship(player);
		}

		public void Hover (bool status) {
			foreach (MemberEntry entry in members.Values) {
				entry.member.Hover(status);
			}
		}

		public void Select (bool status) {
			foreach (MemberEntry entry in members.Values) {
				entry.member.Select(status);
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
			}
		}

		public void Attack (int damage) {
			throw new NotImplementedException();
		}

		public virtual bool CanCommand (string key) {
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

	public class MemberEntry {
		public string key;
		public InfantryMember member;
		public Transform selectionCollider;
		public EventAgent bus;
	}
}