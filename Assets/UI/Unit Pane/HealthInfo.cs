using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsTS.UI {

    public class HealthInfo : MonoBehaviour, IInfoModule {

		public int CurrentHealth {
			get {
				return currentHealth;
			}
			set {
				currentHealth = value;

				FillLevel = (float)currentHealth / MaxHealth;

				text.text = currentHealth + " / " + MaxHealth;
			}
		}

		private int currentHealth = 1;

		public int MaxHealth {
			get {
				return maxHealth;
			}
			set {
				maxHealth = value;

				FillLevel = (float)currentHealth / MaxHealth;

				text.text = CurrentHealth + " / " + maxHealth;
			}
		}

		private int maxHealth = 1;

		private float FillLevel {
			set {
				float rightEdge = literalSize - (literalSize * value);
				barTransform.offsetMax = new Vector2(-rightEdge, 0f);
			}
		}

		public IAttackable CurrentUnit {
			get {
				return currentUnit;
			}
			set {
				currentUnit = value;

				if (currentUnit != null) {
					CurrentHealth = value.Health;
					MaxHealth = value.MaxHealth;
				}
			}
		}

		private IAttackable currentUnit;

		public GameObject GameObject { get { return gameObject; } }

		public string Name { get { return "health"; } }

		private TextMeshProUGUI text;
		private RectTransform barTransform;

		private float literalSize;

		private void Awake () {
			text = transform.Find("HealthNumber").GetComponent<TextMeshProUGUI>();
			barTransform = transform.Find("HealthBar") as RectTransform;

			//xMax is the max literal x co-ords from the center, so if we multiply by 2 that gets us the literal size
			literalSize = barTransform.rect.xMax * 2;
		}

		private void Start () {
			EventBus.AddListener<UnitHurtEvent>(OnEntityHurt);
			EventBus.AddListener<UnitDeathEvent>(OnEntityDeath);
		}

		private void OnEntityHurt (UnitHurtEvent _event) {
			if (ReferenceEquals(_event.Targetable, CurrentUnit)) {
				CurrentHealth = _event.Targetable.Health;
				MaxHealth = _event.Targetable.MaxHealth;
			}
		}

		private void OnEntityDeath (UnitDeathEvent _event) {
			if (ReferenceEquals(_event.Unit, CurrentUnit)) {
				CurrentUnit = null;
			}
		}

		public T Get<T> () {
			if (this is T output) return output;
			return default;
		}
	}
}