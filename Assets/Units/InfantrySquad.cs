using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MarsTS.Units {

	public class InfantrySquad : MonoBehaviour, ISelectable, ITaggable<InfantrySquad>, ICommandable {
		public GameObject GameObject { get { return gameObject; } }

		/*	ISelectable Properties	*/

		public int ID { get { return entityComponent.ID; } }

		public string UnitType { get { return type; } }

		public string RegistryKey { get { return "infantry_squad:" + UnitType; } }

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

		public Commandlet CurrentCommand { get; protected set; }

		public Commandlet[] CommandQueue { get { return commandQueue.ToArray(); } }

		private Queue<Commandlet> commandQueue = new Queue<Commandlet>();

		[SerializeField]
		private string[] boundCommands;

		/*	InfantrySquad Fields	*/

		private List<Infantry> members = new List<Infantry>();
		private List<Transform> selectionColliders = new List<Transform>();

		private Entity entityComponent;

		[SerializeField]
		private int maxMembers;

		[SerializeField]
		private Infantry[] startingMembers;

		[SerializeField]
		private GameObject selectionColliderPrefab;

		private EventAgent bus;

		private void Awake () {
			foreach (Infantry unit in startingMembers) {
				unit.SetOwner(owner);
				unit.squad = this;

				Transform newSelectionCollider = Instantiate(selectionColliderPrefab, transform).transform;
				newSelectionCollider.position = unit.transform.position;
				selectionColliders.Add(newSelectionCollider);

				members.Add(unit);
			}

			entityComponent = GetComponent<Entity>();
			bus = GetComponent<EventAgent>();
		}

		private void Start () {
			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
		}

		private void Update () {
			for (int i = 0; i < members.Count; i++) {
				selectionColliders[i].transform.position = members[i].transform.position;
			}

			UpdateCommands();
		}

		protected void UpdateCommands () {
			if (CurrentCommand is null && commandQueue.TryDequeue(out Commandlet order)) {

				ProcessOrder(order);
			}
		}

		private void OnEntityDeath () {

		}

		protected virtual void ProcessOrder (Commandlet order) {
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

			foreach (Infantry unit in members) {
				unit.Execute(order);
			}
		}

		protected virtual void DistributeOrder (Commandlet order) {
			foreach (Infantry unit in members) {
				unit.Execute(order);
			}
		}

		public string[] Commands () {
			return boundCommands;
		}

		public void Enqueue (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;
			commandQueue.Enqueue(order);
		}

		public void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;
			commandQueue.Clear();

			if (CurrentCommand != null) {
				CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, true, this);
				CurrentCommand.Callback.Invoke(_event);
				bus.Global(_event);
			}

			CurrentCommand = null;
			commandQueue.Enqueue(order);
		}

		public virtual Command Evaluate (ISelectable target) {
			if (target is IAttackable && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get("attack");
			}

			return CommandRegistry.Get("move");
		}

		public virtual Commandlet Auto (ISelectable target) {
			if (target is IAttackable deserialized && target.GetRelationship(owner) == Relationship.Hostile) {
				return CommandRegistry.Get<Attack>("attack").Construct(deserialized);
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
			foreach (Infantry unit in members) {
				unit.Hover(status);
			}
		}

		public void Select (bool status) {
			foreach (Infantry unit in members) {
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
				//info.CurrentUnit = this;
			}
		}
	}
}