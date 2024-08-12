using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Vision;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Flare : MonoBehaviour, ISelectable, ITaggable<Flare>, IAttackable {

        [SerializeField]
        private float lifeTime;

		private float currentLifeTime;

		private EventAgent bus;

		[SerializeField]
		private GameObject[] hideables;

		public GameObject GameObject => gameObject;
		public IUnit Unit => this;

		/*	IAttackable Properties	*/

		public int Health { get { return currentHealth; } }

		public int MaxHealth { get { return maxHealth; } }

		protected int maxHealth = 100;

		protected int currentHealth;

		/*	ISelectable Properties	*/

		public int ID { get { return entityComponent.Id; } }

		public string UnitType { get { return type; } }

		public string RegistryKey { get { return "misc:" + UnitType; } }

		public Sprite Icon { get { return icon; } }

		public Faction Owner { get { return owner; } }

		[SerializeField]
		private Faction owner;

		[SerializeField]
		private string type;

		[SerializeField]
		private Sprite icon;

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(AbstractUnit); } }

		private ISelectable parent;

		private Entity entityComponent;

		private void Awake () {
			bus = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();

			currentHealth = maxHealth;
			currentLifeTime = lifeTime;
		}

		private void Start () {
			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

			EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
			EventBus.AddListener<VisionInitEvent>(OnVisionInit);
		}

		public void Init (ISelectable _parent) {
			parent = _parent;
			owner = parent.Owner;
		}

		private void OnVisionInit (VisionInitEvent _event) {
			bool visible = GameVision.IsVisible(gameObject);

			foreach (GameObject hideable in hideables) {
				hideable.SetActive(visible);
			}
		}

		private void Update () {
			currentLifeTime -= Time.deltaTime;
			currentHealth = Mathf.RoundToInt(maxHealth * (currentLifeTime / lifeTime));

			bus.Global(new UnitHurtEvent(bus, this));

			if (currentLifeTime <= 0) {
				bus.Global(new UnitDeathEvent(bus, this));
				Destroy(gameObject);
			}
		}

		private void OnVisionUpdate (EntityVisibleEvent _event) {
			foreach (GameObject hideable in hideables) {
				hideable.SetActive(_event.Visible);
			}
		}

		public void Select (bool status) {
			bus.Local(new UnitSelectEvent(bus, status));
		}

		public void Hover (bool status) {
			//These are seperated due to the Player Selection Check
			if (status) {
				bus.Local(new UnitHoverEvent(bus, status));
			}
			else if (!Player.Main.HasSelected(this)) {
				bus.Local(new UnitHoverEvent(bus, status));
			}
		}

		public Relationship GetRelationship (Faction other) {
			return Owner.GetRelationship(other);
		}

		public bool SetOwner (Faction player) {
			owner = player;
			return true;
		}

		public Flare Get () {
			return this;
		}

		private void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				HealthInfo info = _event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;
			}
		}

		public void Attack (int damage) {
			throw new NotImplementedException();
		}
	}
}