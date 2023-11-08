using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Units;
using MarsTS.Units.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace MarsTS.Buildings {

	public class Building : MonoBehaviour, ISelectable, ITaggable<Building> {

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

		public string BuildingType => type;

		public int Health {
			get {
				return currentHealth;
			}
		}

		private int currentHealth;

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
		private string[] boundCommands;

		[SerializeField]
		private GameObject selectionCircle;

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

		private bool isConstructed;

		public bool Ready {
			get {
				return isConstructed;
			}
		}

		[SerializeField]
		private GameObject ghost;

		public GameObject SelectionGhost {
			get {
				return ghost;
			}
		}

		private EventAgent eventAgent;

		Transform model;

		protected virtual void Awake () {
			selectionCircle.SetActive(false);
			entityComponent = GetComponent<Entity>();
			currentHealth = 1;

			eventAgent = GetComponent<EventAgent>();

			model = transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;
		}

		public string[] Commands () {
			return boundCommands;
		}

		public void Enqueue (Commandlet order) {
			throw new System.NotImplementedException();
		}

		public void Execute (Commandlet order) {
			throw new System.NotImplementedException();
		}

		public Building Get () {
			return this;
		}

		public Relationship GetRelationship (Faction other) {
			return owner.GetRelationship(other);
		}

		public string Name () {
			return BuildingType;
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
				currentHealth = (int)(maxHealth * progress);
				//eventAgent.Local(new BuildingConstructStepEvent(eventAgent, this));

				model.localScale = Vector3.one * progress;

				eventAgent.Local(new EntityHurtEvent(eventAgent, this));
			}

			currentHealth -= damage;

			if (currentHealth <= 0) {
				Destroy(gameObject);
			}
		}
	}
}