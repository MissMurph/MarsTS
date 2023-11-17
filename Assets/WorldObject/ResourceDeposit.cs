using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
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

		public string UnitType { get { return "world_object"; } }

		public string RegistryKey { get { return UnitType + ":" + key; } }

		[SerializeField]
		private string key;

		public Faction Owner { get { return null; } }

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(ResourceDeposit); } }

		/*	IHarvestable Properties	*/

		public int OriginalAmount { get { return startingAmount; } }

		private int startingAmount;

		public int StoredAmount { get { return attribute.Amount; } }

		/*	Deposit Fields	*/

		private Entity entityComponent;

		private EventAgent bus;

		GameObject selectionCircle;

		//This is just for the registry key, some examples:
		//deposit:scrap
		//deposit:oil_slick
		//deposit:shale_oil
		//deposit:biomass
		//deposit:rock
		[SerializeField]
		private string depositType;

		private EntityAttribute attribute; 

		private void Awake () {
			entityComponent = GetComponent<Entity>();
			attribute = GetComponent<EntityAttribute>();
			bus = GetComponent<EventAgent>();
			selectionCircle = transform.Find("SelectionCircle").gameObject;
			selectionCircle.SetActive(false);
		}

		private void Start () {
			selectionCircle.GetComponent<Renderer>().material = GetRelationship(Player.Main).Material();
			startingAmount = attribute.Amount;
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

		public int Harvest (string resourceKey, int harvestAmount, Func<int, int> extractor) {
			int availableAmount = Mathf.Min(harvestAmount, attribute.Amount);

			int finalAmount = extractor(availableAmount);

			if (finalAmount > 0) {
				bus.Global(new ResourceHarvestedEvent(bus, this, finalAmount, resourceKey));
				attribute.Amount -= finalAmount;
			}

			return finalAmount;
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
			return false;
		}
	}
}