using MarsTS.Entities;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.World {

	public class ResourceDeposit : MonoBehaviour, IHarvestable, ISelectable, ITaggable<ResourceDeposit> {

		public GameObject GameObject { get { return gameObject; } }

		/*	ISelectable Properties	*/

		public int ID { get { return entityComponent.ID; } }

		public string UnitType { get { return "deposit"; } }

		public string RegistryKey { get { return UnitType + ":" + depositType; } }

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(ResourceDeposit); } }

		/*	Deposit Fields	*/

		private Entity entityComponent;

		//This is just for the registry key, some examples:
		//deposit:scrap
		//deposit:oil_slick
		//deposit:shale_oil
		//deposit:biomass
		//deposit:rock
		[SerializeField]
		private string depositType;

		private void Awake () {
			entityComponent = GetComponent<Entity>();
		}

		public bool CanHarvest (string resourceKey, ISelectable unit) {
			return true;
		}

		public ResourceDeposit Get () {
			return this;
		}

		public Relationship GetRelationship (Faction player) {
			return Relationship.Neutral;
		}

		public int Harvest (string resourceKey, int amount) {
			throw new System.NotImplementedException();
		}

		public void Hover (bool status) {
			throw new System.NotImplementedException();
		}

		public void Select (bool status) {
			throw new System.NotImplementedException();
		}

		public bool SetOwner (Faction player) {
			throw new System.NotImplementedException();
		}
	}
}