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

namespace MarsTS.Buildings {

	public abstract class Building : MonoBehaviour, ISelectable, ITaggable<Building>, IRegistryObject<Building> {

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
		protected Faction owner;

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

		private GameObject selectionCircle;

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

		protected EventAgent bus;

		protected Transform model;

		protected virtual void Awake () {
			selectionCircle = transform.Find("SelectionCircle").gameObject;
			selectionCircle.SetActive(false);
			Health = 1;

			bus = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();

			model = transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;
		}

		protected virtual void CancelConstruction () {
			if (!Constructed) {
				bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject, 0.1f);
			}
		}

		public string[] Commands () {
			if (!Constructed) {
				return new string[] { "cancelConstruction" };
			}
			else return boundCommands;
		}

		public abstract void Enqueue (Commandlet order);

		public abstract void Execute (Commandlet order);

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
			selectionCircle.SetActive(status);
			bus.Local(new UnitSelectEvent(bus, status));
		}

		public void Hover (bool status) {
			//These are seperated due to the Player Selection Check
			if (status) {
				selectionCircle.SetActive(true);
				bus.Local(new UnitHoverEvent(bus, status));
			}
			else if (!Player.Main.HasSelected(this)) {
				selectionCircle.SetActive(false);
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

				bus.Global(new EntityHurtEvent(bus, this));

				if (progress >= 1f) bus.Global(new CommandsUpdatedEvent(bus, this, boundCommands));
				return;
			}

			Health -= damage;

			if (Health <= 0) {
				bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject, 0.1f);
			}
		}

		public IRegistryObject<Building> GetRegistryEntry () {
			throw new NotImplementedException();
		}
	}
}