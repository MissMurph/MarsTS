using MarsTS.Events;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MarsTS.UI {

    public class ResourceInfo : MonoBehaviour, IInfoModule {

		public int CurrentValue {
			get {
				return currentValue;
			}
			set {
				currentValue = value;

				FillLevel = (float)currentValue / MaxValue;

				text.text = currentValue + " / " + MaxValue;
			}
		}

		private int currentValue = 1;

		public int MaxValue {
			get {
				return maxValue;
			}
			set {
				maxValue = value;

				FillLevel = (float)currentValue / MaxValue;

				text.text = CurrentValue + " / " + maxValue;
			}
		}

		private int maxValue = 1;

		private float FillLevel {
			set {
				float rightEdge = literalSize - (literalSize * value);
				barTransform.offsetMax = new Vector2(-rightEdge, 0f);
			}
		}

		public IHarvestable CurrentDeposit {
			get {
				return currentDeposit;
			}
			set {
				currentDeposit = value;

				CurrentValue = value.StoredAmount;
				MaxValue = value.OriginalAmount;
			}
		}

		private IHarvestable currentDeposit;

		public GameObject GameObject { get { return gameObject; } }

		public string Name { get { return "deposit"; } }

		private TextMeshProUGUI text;
		private RectTransform barTransform;

		private float literalSize;

		private void Awake () {
			text = transform.Find("Number").GetComponent<TextMeshProUGUI>();
			barTransform = transform.Find("Bar") as RectTransform;

			//xMax is the max literal x co-ords from the center, so if we multiply by 2 that gets us the literal size
			literalSize = barTransform.rect.xMax * 2;
		}

		private void Start () {
			EventBus.AddListener<UnitDeathEvent>(OnEntityDeath);
			EventBus.AddListener<ResourceHarvestedEvent>(OnResourceHarvested);
		}

		private void OnResourceHarvested (ResourceHarvestedEvent _event) {
			if (_event.EventSide == ResourceHarvestedEvent.Side.Deposit && ReferenceEquals(_event.Unit, CurrentDeposit)) {
				CurrentValue = _event.Unit.StoredAmount;
				MaxValue = _event.Unit.OriginalAmount;
			}
		}

		private void OnEntityDeath (UnitDeathEvent _event) {
			if (ReferenceEquals(_event.Unit, CurrentDeposit)) {
				CurrentDeposit = null;
			}
		}

		public T Get<T> () {
			if (this is T output) return output;
			return default;
		}
	}
}