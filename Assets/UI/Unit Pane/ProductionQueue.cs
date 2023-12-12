using MarsTS.Commands;
using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsTS.UI {

    public class ProductionQueue : MonoBehaviour, IInfoModule {

		public int CurrentProduction {
			get {
				return currentProduction;
			}
			set {
				currentProduction = value;

				FillLevel = (float)currentProduction / MaxProduction;

				text.text = currentProduction + " / " + MaxProduction;
			}
		}

		private int currentProduction = 1;

		public int MaxProduction {
			get {
				return maxProduction;
			}
			set {
				maxProduction = value;

				FillLevel = (float)CurrentProduction / maxProduction;

				text.text = CurrentProduction + " / " + maxProduction;
			}
		}

		private int maxProduction = 1;

		private float FillLevel {
			set {
				float rightEdge = literalSize - (literalSize * value);
				barTransform.offsetMax = new Vector2(-rightEdge, 0f);
			}
		}

		private Image currentProdIcon;
		
		[SerializeField]
		private GameObject[] queueObjects;

		private Image[] queueIcons;

		private TextMeshProUGUI overflow;

		private GameObject productionProgress;

		private TextMeshProUGUI text;
		private RectTransform barTransform;

		private float literalSize;

		public GameObject GameObject { get { return gameObject; } }

		public string Name { get { return "productionQueue"; } }

		private ICommandable currentUnit;

		public T Get<T> () {
			if (this is T output) return output;
			return default;
		}

		private void Awake () {
			productionProgress = transform.Find("Production").gameObject;
			text = productionProgress.GetComponentInChildren<TextMeshProUGUI>();
			barTransform = productionProgress.transform.Find("ProductionBar") as RectTransform;
			literalSize = barTransform.rect.xMax * 2;

			overflow = transform.Find("OverflowCounter").Find("Counter").GetComponent<TextMeshProUGUI>();
			currentProdIcon = transform.Find("CurrentOrder").Find("Icon").GetComponent<Image>();

			queueIcons = new Image[queueObjects.Length];

			for (int i = 0; i < queueObjects.Length; i++) {
				queueIcons[i] = queueObjects[i].transform.Find("Icon").GetComponent<Image>();
			}

			foreach (GameObject queueObject in queueObjects) {
				queueObject.SetActive(false);
			}

			currentProdIcon.transform.parent.gameObject.SetActive(false);
			overflow.transform.parent.gameObject.SetActive(false);
			productionProgress.SetActive(false);
		}

		private void Start () {
			EventBus.AddListener<UnitProducedEvent>(OnUnitProduction);
			EventBus.AddListener<ProductionEvent>(OnProductionStep);
			EventBus.AddListener<ProductionEvent>(OnProductionUpdate);
		}

		private void OnProductionUpdate (ProductionEvent _event) {
			if (_event.Name != "productionStarted" && _event.Name != "productionQueued") return;
			if (ReferenceEquals(_event.Producer, currentUnit)) {
				SetQueue(currentUnit, _event.CurrentProduction, _event.ProductionQueue);
			}
		}

		private void OnProductionStep (ProductionEvent _event) {
			if (_event.Name != "productionStep") return;
			if (ReferenceEquals(_event.Producer, currentUnit)) {
				CurrentProduction = _event.CurrentProduction.ProductionProgress;
				MaxProduction = _event.CurrentProduction.ProductionRequired;
			}
		}

		private void OnUnitProduction (UnitProducedEvent _event) {
			if (ReferenceEquals(_event.Producer, currentUnit)) {
				ProductionCommandlet currentProd = _event.Producer.CurrentCommand as ProductionCommandlet;
				ProductionCommandlet[] queue = _event.Producer.CommandQueue as ProductionCommandlet[];

				if (currentProd != null && queue.Length > 0) {
					SetQueue(currentUnit, currentProd, queue);
				}
				else {
					currentProdIcon.transform.parent.gameObject.SetActive(false);
					overflow.transform.parent.gameObject.SetActive(false);
					productionProgress.SetActive(false);
				}
			}
		}

		

		public void SetQueue (ICommandable unit, ProductionCommandlet current, ProductionCommandlet[] queue) {
			currentUnit = unit;

			if (current != null) {
				currentProdIcon.sprite = current.Unit.Icon;
				currentProdIcon.transform.parent.gameObject.SetActive(true);
				productionProgress.SetActive(true);

				int orders = queue.Length;

				CurrentProduction = current.ProductionProgress;
				MaxProduction = current.ProductionRequired;

				for (int i = 0; i < orders; i++) {
					if (i < queueIcons.Length) {
						queueIcons[i].sprite = queue[i].Unit.Icon;
						queueObjects[i].SetActive(true);
					}
					else {
						queueObjects[queueObjects.Length - 1].SetActive(false);
						overflow.text = "+" + (orders - queueObjects.Length);
						overflow.transform.parent.gameObject.SetActive(true);
					}
				}

				if (orders < queueObjects.Length) {
					for (int i = orders; i < queueObjects.Length; i++) {
						queueObjects[i].SetActive(false);
					}
				}
			}
		}
	}
}