using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.UI;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarsTS.Buildings {

    public class Landmines : Building {

        private Dictionary<string, Landmine> childMines;
		private Dictionary<string, Collider> entityColliders;
		private Dictionary<string, Transform> selectionColliders;

		private int collectiveMaxHealth;

		[SerializeField]
		private GameObject entityColliderPrefab;

		[SerializeField]
		private GameObject selectionColliderPrefab;

		public override int MaxHealth { get { return collectiveMaxHealth; } }

		public override int Health {
			get {
				if (Constructed) {
					int output = 0;

					foreach (Landmine child in childMines.Values) {
						output += child.Health;
					}

					return output;
				}
				else {
					return base.Health;
				}
			}
		}


		protected override void Awake () {
			Health = 1;
			commands = GetComponent<CommandQueue>();

			bus = GetComponent<EventAgent>();
			entityComponent = GetComponent<Entity>();

			childMines = new Dictionary<string, Landmine>();
			entityColliders = new Dictionary<string, Collider>();
			selectionColliders = new Dictionary<string, Transform>();

			Landmine[] foundChildren = GetComponentsInChildren<Landmine>();

			collectiveMaxHealth = 0;

			foreach (Landmine child in foundChildren) {
				collectiveMaxHealth += child.MaxHealth;
				RegisterMine(child);
			}
        }

		protected override void Start () {
			base.Start();
		}

		private void RegisterMine (Landmine child) {
			child.SetOwner(owner);

			EventAgent unitEvents = child.GetComponent<EventAgent>();
			unitEvents.AddListener<EntityInitEvent>(OnChildInit);
			unitEvents.AddListener<UnitDeathEvent>(OnMineDestroyed);
			unitEvents.AddListener<UnitHurtEvent>(OnChildHurt);

			child.SetConstructionProgress(currentWork);

			model = child.transform.Find("Model");

			if (currentWork > 0) {
				model.localScale = Vector3.one * (currentWork / constructionWork);
				Health = maxHealth * (int)(currentWork / constructionWork);
			}
			else model.localScale = Vector3.zero;
		}

		private void OnChildInit (EntityInitEvent _event) {
			if (_event.Phase == Phase.Post) return;

			childMines[_event.ParentEntity.gameObject.name] = _event.ParentEntity.Get<Landmine>("selectable");

			Collider newEntityCollider = Instantiate(entityColliderPrefab, _event.ParentEntity.gameObject.transform.localPosition, _event.ParentEntity.gameObject.transform.localRotation, transform).GetComponent<Collider>();
			Transform newSelectionCollider = Instantiate(selectionColliderPrefab, _event.ParentEntity.gameObject.transform.localPosition, _event.ParentEntity.gameObject.transform.localRotation, transform).transform;

			entityColliders[_event.ParentEntity.gameObject.name] = newEntityCollider;
			selectionColliders[_event.ParentEntity.gameObject.name] = newSelectionCollider;

			if (currentWork >= constructionWork) {
				newEntityCollider.isTrigger = true;
				newEntityCollider.transform.SetParent(childMines[_event.ParentEntity.gameObject.name].transform);
			}

			childMines[_event.ParentEntity.gameObject.name].transform.SetParent(null, true);
		}

		private void OnMineDestroyed (UnitDeathEvent _event) {
			string key = _event.Unit.GameObject.name;

			childMines.Remove(key);

			Destroy(selectionColliders[key].gameObject);

			selectionColliders.Remove(key);

			if (childMines.Count <= 0) {
				bus.Global(new UnitDeathEvent(bus, this));
				Destroy(gameObject);
			}
		}

		private void OnChildHurt (UnitHurtEvent _event) {
			if (!Constructed) return;
			bus.Global(new UnitHurtEvent(bus, this));
		}

		public override void Hover (bool status) {
			foreach (Landmine child in childMines.Values) {
				child.Hover(status);
			}
		}

		public override void Select (bool status) {
			foreach (Landmine child in childMines.Values) {
				child.Select(status);
			}
		}

		public override void Attack (int damage) {
			if (currentWork < constructionWork && damage < 0) {
				float previousProgress = (float)currentWork / constructionWork;

				currentWork -= damage;

				float progress = (float)currentWork / constructionWork;

				base.Health += Mathf.RoundToInt(maxHealth * (progress - previousProgress));

				model.localScale = Vector3.one * progress;

				bus.Global(new UnitHurtEvent(bus, this));

				foreach (Landmine child in childMines.Values) {
					child.Attack(damage);
				}

				if (progress >= 1f) {
					bus.Global(new CommandsUpdatedEvent(bus, this, boundCommands));

					foreach (KeyValuePair<string, Collider> colliderEntry in entityColliders) {
						colliderEntry.Value.isTrigger = true;
						colliderEntry.Value.transform.SetParent(childMines[colliderEntry.Key].transform);
					}

					return;
				}
			}

			if (base.Health <= 0) return;
			if (damage < 0 && base.Health >= maxHealth) return;
			base.Health -= damage;
			bus.Global(new UnitHurtEvent(bus, this));

			if (base.Health <= 0) {
				bus.Global(new UnitDeathEvent(bus, this));
				Destroy(gameObject, 0.1f);
			}
		}

		protected override void OnUnitInfoDisplayed (UnitInfoEvent _event) {
			if (ReferenceEquals(_event.Unit, this)) {
				HealthInfo info = _event.Info.Module<HealthInfo>("health");
				info.CurrentUnit = this;
			}
		}
	}
}