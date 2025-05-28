using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI.Unit_Pane {

    public class ProductionInfo : MonoBehaviour, IInfoModule {

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
			EventBus.AddListener<ProductionStepCompleteEvent>(OnUnitProduction);
			EventBus.AddListener<ProductionStepEvent>(OnProductionStep);
			EventBus.AddListener<ProductionStepEvent>(OnProductionUpdate);
		}

		private void OnProductionUpdate (ProductionStepEvent stepEvent) {
			if (stepEvent.Name != "productionStarted" && stepEvent.Name != "productionQueued") return;
			if (ReferenceEquals(stepEvent.Producer, currentUnit)) {
				SetQueue(currentUnit, stepEvent.CurrentProduction, stepEvent.Queue.QueuedProduction);
			}
		}

		private void OnProductionStep (ProductionStepEvent stepEvent) {
			if (stepEvent.Name != "productionStep") return;
			if (ReferenceEquals(stepEvent.Producer, currentUnit)) {
				CurrentProduction = stepEvent.CurrentProduction.ProductionProgress;
				MaxProduction = stepEvent.CurrentProduction.ProductionRequired;
			}
		}

		private void OnUnitProduction (ProductionStepCompleteEvent _event) {
			if (ReferenceEquals(_event.Producer, currentUnit)) {
				IProducable currentProd = _event.CurrentProduction;
				IProducable[] queue = _event.Queue.QueuedProduction;

				if (currentProd != null && queue.Length > 0) {
					SetQueue(currentUnit, currentProd, queue);
				}
				else {
					Deactivate();
				}
			}
		}

		public void Deactivate () {
			currentProdIcon.transform.parent.gameObject.SetActive(false);
			overflow.transform.parent.gameObject.SetActive(false);
			productionProgress.SetActive(false);

			for (int i = 0; i < queueObjects.Length; i++) {
				queueObjects[i].SetActive(false);
			}
			
			gameObject.SetActive(false);
		}

		public void SetQueue (ICommandable unit, IProducable current, IProducable[] queue) {
			currentUnit = unit;

			if (current != null) {
				currentProdIcon.sprite = current.Get().Command.Icon;
				currentProdIcon.transform.parent.gameObject.SetActive(true);
				productionProgress.SetActive(true);

				int orders = queue.Length;

				CurrentProduction = current.ProductionProgress;
				MaxProduction = current.ProductionRequired;

				for (int i = 0; i < orders; i++) {
					if (i < queueIcons.Length) {
						queueIcons[i].sprite = queue[i].Get().Command.Icon;
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
			else {
				Deactivate();
			}
		}
	}
}