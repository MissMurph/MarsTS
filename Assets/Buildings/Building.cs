using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Prefabs;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.Units.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace MarsTS.Buildings {

	public class Building : MonoBehaviour, ISelectable, ITaggable<Building>, IRegistryObject<Building> {

		public GameObject GameObject {
			get { 
				return gameObject; 
			}
		}

		public int ID {
			get {
				return entityComponent.ID;
			}
		}

		private Entity entityComponent;

		public string Key {
			get {
				return "building";
			}
		}

		public Type Type {
			get {
				return typeof(Building);
			}
		}

		[SerializeField]
		private Faction owner;

		public int Health { get; protected set; }

		public int MaxHealth {
			get {
				return maxHealth;
			}
		}

		[SerializeField]
		private int maxHealth;

		[Header("Entity Fields")]

		[SerializeField]
		private string type;

		[SerializeField]
		private GameObject selectionCircle;

		/*	Commands	*/
		public Queue<Commandlet> CommandQueue = new Queue<Commandlet>();

		public Commandlet CurrentCommand { get; protected set; }

		[SerializeField]
		private string[] boundCommands;

		[Header("Building Fields")]

		[SerializeField]
		private int constructionWork;

		public int ConstructionProgress {
			get {
				return currentWork;
			}
		}

		public int ConstructionRequired {
			get {
				return constructionWork;
			}
		}

		[SerializeField]
		private int currentWork;

		public bool Constructed {
			get {
				return currentWork >= constructionWork;
			}
		}

		[SerializeField]
		private GameObject ghost;

		public GameObject SelectionGhost {
			get {
				return ghost;
			}
		}

		public string RegistryType => "building";

		public string RegistryKey => RegistryType + ":" + UnitType;

		public string UnitType => type;

		private EventAgent eventAgent;

		Transform model;

		protected virtual void Awake () {
			selectionCircle.SetActive(false);
			Health = 1;

			eventAgent = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();

			model = transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;
		}

		protected virtual void Update () {
			UpdateCommands();
		}

		protected void UpdateCommands () {
			if (CurrentCommand is null && CommandQueue.TryDequeue(out Commandlet order)) {

				CurrentCommand = order;

				ProcessOrder(order);
			}
		}

		protected virtual void CancelConstruction () {
			if (!Constructed) {
				eventAgent.Global(new EntityDeathEvent(eventAgent, this));
				Destroy(gameObject, 0.1f);
			}
		}

		public string[] Commands () {
			if (!Constructed) {
				return new string[] { "cancelConstruction" };
			}
			else return boundCommands;
		}

		public void Enqueue (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;
			CommandQueue.Enqueue(order);
		}

		public void Execute (Commandlet order) {
			if (!GetRelationship(Player.Main).Equals(Relationship.Owned)) return;
			CommandQueue.Clear();
			CurrentCommand = null;
			CommandQueue.Enqueue(order);
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
			return owner.GetRelationship(other);
		}

		public void Select (bool status) {
			if (status) selectionCircle.SetActive(true);
			else selectionCircle.SetActive(false);
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

				eventAgent.Global(new EntityHurtEvent(eventAgent, this));

				if (progress >= 1f) eventAgent.Global(new CommandsUpdatedEvent(eventAgent, this, boundCommands));
				return;
			}

			Health -= damage;

			if (Health <= 0) {
				eventAgent.Global(new EntityDeathEvent(eventAgent, this));
				Destroy(gameObject, 0.1f);
			}
		}

		public IRegistryObject<Building> GetRegistryEntry () {
			throw new NotImplementedException();
		}
	}
}