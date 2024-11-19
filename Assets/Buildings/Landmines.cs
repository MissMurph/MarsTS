using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.UI;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class Landmines : Building {

        private Dictionary<string, Landmine> _childMines;
		private Dictionary<string, Collider> _entityColliders;
		private Dictionary<string, Transform> _selectionColliders;
		private Dictionary<string, Transform> _detectableColliders;

		private int _collectiveMaxHealth;

		[SerializeField]
		private GameObject entityColliderPrefab;

		[SerializeField]
		private GameObject selectionColliderPrefab;

		[SerializeField]
		private GameObject dummyColliderPrefab;

		public override int MaxHealth => _collectiveMaxHealth;

		public override int Health {
			get
			{
				if (!Constructed) 
					return base.Health;
				
				int output = 0;

				foreach (Landmine child in _childMines.Values) {
					output += child.Health;
				}

				return output;
			}
		}

		//private List<GameObject> dummysToDestroy = new List<GameObject>();

		protected override void Awake () {
			Health = 0;
			commands = GetComponent<CommandQueue>();

			bus = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();

			_childMines = new Dictionary<string, Landmine>();
			_entityColliders = new Dictionary<string, Collider>();
			_selectionColliders = new Dictionary<string, Transform>();
			_detectableColliders = new Dictionary<string, Transform>();

			Landmine[] foundChildren = GetComponentsInChildren<Landmine>();

			_collectiveMaxHealth = 0;

			foreach (Landmine child in foundChildren) {
				healthPerConstructionPoint = child.MaxHealth / constructionWork;
				_collectiveMaxHealth += child.MaxHealth;
				RegisterMine(child);
			}
		}

		private void LateUpdate () {
			foreach (KeyValuePair<string, Transform> dummy in _detectableColliders) {
				dummy.Value.position = _childMines[dummy.Key].transform.position;
			}
		}

		private void RegisterMine (Landmine child) {
			EventAgent unitEvents = child.GetComponent<EventAgent>();
			unitEvents.AddListener<EntityInitEvent>(OnChildInit);
			unitEvents.AddListener<UnitDeathEvent>(OnMineDestroyed);
			unitEvents.AddListener<UnitHurtEvent>(OnChildHurt);
			unitEvents.AddListener<EntityVisibleEvent>(OnChildVisionUpdate);

			child.SetConstructionProgress(currentWork);

			model = child.transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
				Health = MaxHealth * (int)(currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;

			bus.Local(new SquadRegisterEvent(bus, this, child));
		}

		private void OnChildInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Post) return;

			_childMines[_event.ParentEntity.gameObject.name] = _event.ParentEntity.Get<Landmine>("selectable");

			_childMines[_event.ParentEntity.gameObject.name].SetOwner(Owner);

			Collider newEntityCollider = Instantiate(entityColliderPrefab, _event.ParentEntity.gameObject.transform.position, _event.ParentEntity.gameObject.transform.rotation, transform).GetComponent<Collider>();
			Transform newSelectionCollider = Instantiate(selectionColliderPrefab, _event.ParentEntity.gameObject.transform.position, _event.ParentEntity.gameObject.transform.rotation, transform).transform;
			Transform newDummyCollider = Instantiate(dummyColliderPrefab, _event.ParentEntity.gameObject.transform.position, _event.ParentEntity.gameObject.transform.rotation, transform).transform;

			_entityColliders[_event.ParentEntity.gameObject.name] = newEntityCollider;
			_selectionColliders[_event.ParentEntity.gameObject.name] = newSelectionCollider;
			_detectableColliders[_event.ParentEntity.gameObject.name] = newDummyCollider;

			if (currentWork >= constructionWork) {
				newEntityCollider.isTrigger = true;
				newEntityCollider.transform.SetParent(_childMines[_event.ParentEntity.gameObject.name].transform);
			}

			_childMines[_event.ParentEntity.gameObject.name].transform.SetParent(null, true);
		}

		private void OnMineDestroyed (UnitDeathEvent _event) {
			string key = _event.Unit.GameObject.name;

			_childMines.Remove(key);

			Destroy(_selectionColliders[key].gameObject);
			_detectableColliders[key].position = Vector3.down * 1000f;

			_selectionColliders.Remove(key);
			_detectableColliders.Remove(key);

			if (_childMines.Count <= 0) {
				bus.Global(new UnitDeathEvent(bus, this));
				Destroy(gameObject);
			}
			else {
				foreach (Transform dummy in _detectableColliders.Values) {
					dummy.position = Vector3.down * 1000f;
				}
			}
		}

		private void OnChildHurt (UnitHurtEvent _event) {
			if (!Constructed) return;
			bus.Global(new UnitHurtEvent(bus, this));
		}

		private void OnChildVisionUpdate (EntityVisibleEvent _event) {
			if (_selectionColliders.TryGetValue(_event.UnitName, out Transform collider)) {
				collider.gameObject.SetActive(_event.Visible);
			}
		}

		public override void Hover (bool status) {
			foreach (Landmine child in _childMines.Values) {
				child.Hover(status);
			}
		}

		public override void Select (bool status) {
			foreach (Landmine child in _childMines.Values) {
				child.Select(status);
			}
		}

		public override void Attack (int damage) {
			if (currentWork < constructionWork && damage < 0) {
				currentWork -= damage;

				float progress = (float)currentWork / constructionWork;

				base.Health += healthPerConstructionPoint;

				model.localScale = Vector3.one * progress;

				bus.Global(new UnitHurtEvent(bus, this));

				foreach (Landmine child in _childMines.Values) {
					child.Attack(damage);
				}

				if (progress >= 1f) {
					bus.Global(new CommandsUpdatedEvent(bus, this, boundCommands));

					foreach (KeyValuePair<string, Collider> colliderEntry in _entityColliders) {
						colliderEntry.Value.isTrigger = true;
						colliderEntry.Value.transform.SetParent(_childMines[colliderEntry.Key].transform);
					}

					return;
				}
			}

			if (base.Health <= 0) return;
			if (damage < 0 && base.Health >= MaxHealth) return;
			base.Health -= damage;
			bus.Global(new UnitHurtEvent(bus, this));

			if (base.Health <= 0) {
				bus.Global(new UnitDeathEvent(bus, this));
				Destroy(gameObject, 0.1f);
			}
		}

		protected override void OnUnitInfoDisplayed (UnitInfoEvent @event) {
			if (ReferenceEquals(@event.Unit, this)) {
				HealthInfo info = @event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;
			}
		}
	}
}