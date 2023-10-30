using MarsTS.Entities;
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

		private Faction owner;

		public string BuildingType {
			get {
				return type;
			}
		}

		[Header("Entity Fields")]

		[SerializeField]
		private string type;

		[SerializeField]
		private string[] boundCommands;

		[SerializeField]
		private GameObject selectionCircle;

		[Header("Building Fields")]

		[SerializeField]
		private float constructionWork;

		private float currentWork;

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

		protected virtual void Awake () {
			selectionCircle.SetActive(false);
			entityComponent = GetComponent<Entity>();
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
	}
}