using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Vision;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class Flare : MonoBehaviour, ISelectable, ITaggable<Flare> {

        [SerializeField]
        private float lifeTime;

		private EventAgent bus;

		[SerializeField]
		private GameObject[] hideables;

		public GameObject GameObject => throw new System.NotImplementedException();

		public int ID => throw new System.NotImplementedException();

		public string UnitType => throw new System.NotImplementedException();

		public string RegistryKey => throw new System.NotImplementedException();

		public Faction Owner { get { return owner; } }

		[SerializeField]
		private Faction owner;

		public Sprite Icon => throw new System.NotImplementedException();

		/*	ITaggable Properties	*/

		public string Key { get { return "selectable"; } }

		public Type Type { get { return typeof(Unit); } }

		private ISelectable parent;

		private void Awake () {
			bus = GetComponent<EventAgent>();
		}

		private void Start () {
			bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

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
			lifeTime -= Time.deltaTime;

			if (lifeTime <= 0) {
				//bus.Global(new EntityDeathEvent(bus, this));
				Destroy(gameObject);
			}
		}

		private void OnVisionUpdate (EntityVisibleEvent _event) {
			foreach (GameObject hideable in hideables) {
				hideable.SetActive(_event.Visible);
			}
		}

		public void Select (bool status) {
			throw new System.NotImplementedException();
		}

		public void Hover (bool status) {
			throw new System.NotImplementedException();
		}

		public Relationship GetRelationship (Faction player) {
			throw new System.NotImplementedException();
		}

		public bool SetOwner (Faction player) {
			throw new System.NotImplementedException();
		}

		public Flare Get () {
			throw new NotImplementedException();
		}
	}
}