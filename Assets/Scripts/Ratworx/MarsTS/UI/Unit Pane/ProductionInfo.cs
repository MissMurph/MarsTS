using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Units;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Ratworx.MarsTS.UI.Unit_Pane {

    public class ProductionInfo : MonoBehaviour, IInfoModule {

		public int CurrentProduction {
			get => _currentProduction;
			set {
				_currentProduction = value;

				FillLevel = (float)_currentProduction / MaxProduction;

				_text.text = _currentProduction + " / " + MaxProduction;
			}
		}

		private int _currentProduction = 1;

		public int MaxProduction {
			get => _maxProduction;
			set {
				_maxProduction = value;

				FillLevel = (float)CurrentProduction / _maxProduction;

				_text.text = CurrentProduction + " / " + _maxProduction;
			}
		}

		private int _maxProduction = 1;

		private float FillLevel {
			set {
				float rightEdge = _literalSize - (_literalSize * value);
				_barTransform.offsetMax = new Vector2(-rightEdge, 0f);
			}
		}

		private Image _currentProdIcon;
		
		[FormerlySerializedAs("queueObjects")]
		[SerializeField]
		private GameObject[] _queueObjects;

		private Image[] _queueIcons;

		private TextMeshProUGUI _overflow;

		private GameObject _productionProgress;

		private TextMeshProUGUI _text;
		private RectTransform _barTransform;

		private float _literalSize;

		public GameObject GameObject => gameObject;

		public string Name => "productionQueue";

		private ProductionQueue _currentSelectedQueue;

		public T Get<T> () {
			if (this is T output) return output;
			return default;
		}

		private void Awake () {
			_productionProgress = transform.Find("Production").gameObject;
			_text = _productionProgress.GetComponentInChildren<TextMeshProUGUI>();
			_barTransform = _productionProgress.transform.Find("ProductionBar") as RectTransform;
			_literalSize = _barTransform.rect.xMax * 2;

			_overflow = transform.Find("OverflowCounter").Find("Counter").GetComponent<TextMeshProUGUI>();
			_currentProdIcon = transform.Find("CurrentOrder").Find("Icon").GetComponent<Image>();

			_queueIcons = new Image[_queueObjects.Length];

			for (int i = 0; i < _queueObjects.Length; i++) {
				_queueIcons[i] = _queueObjects[i].transform.Find("Icon").GetComponent<Image>();
			}

			foreach (GameObject queueObject in _queueObjects) {
				queueObject.SetActive(false);
			}

			_currentProdIcon.transform.parent.gameObject.SetActive(false);
			_overflow.transform.parent.gameObject.SetActive(false);
			_productionProgress.SetActive(false);
		}

		private void OnProductionStep (ProductionStepEvent stepEvent) {
			CurrentProduction = (int)_currentSelectedQueue.CurrentProductionAmount;
			MaxProduction = _currentSelectedQueue.CurrentOrder.ProductionRequired;
		}

		public void Deactivate () {
			_currentProdIcon.transform.parent.gameObject.SetActive(false);
			_overflow.transform.parent.gameObject.SetActive(false);
			_productionProgress.SetActive(false);

			for (int i = 0; i < _queueObjects.Length; i++) {
				_queueObjects[i].SetActive(false);
			}
			
			gameObject.SetActive(false);
		}

		public void SetQueue (ProductionQueue productionQueue) {
			if (_currentSelectedQueue is not null) {
				EntityCache.TryGetEntityComponent(_currentSelectedQueue.gameObject.name, out EventAgent eventAgent);
				eventAgent.RemoveListener<ProductionStepEvent>(OnProductionStep);
				productionQueue.OnQueueChanged -= UpdateQueue;
			}
			
			_currentSelectedQueue = productionQueue;

			if (_currentSelectedQueue is not null) {
				EntityCache.TryGetEntityComponent(_currentSelectedQueue.gameObject.name, out EventAgent eventAgent);
				eventAgent.AddListener<ProductionStepEvent>(OnProductionStep);
				_currentSelectedQueue.OnQueueChanged += UpdateQueue;
				
				UpdateQueue();
			}
		}

		private void UpdateQueue() {
			if (_currentSelectedQueue.QueueCount <= 0) {
				Deactivate();
				return;
			}
			
			// TODO: Create a unit icon registry
			ISelectable currentOrderSelectable = GetSelectableFromProductionOrder(_currentSelectedQueue.CurrentOrder);

			_currentProdIcon.sprite = currentOrderSelectable.Icon;
			_currentProdIcon.transform.parent.gameObject.SetActive(true);
			_productionProgress.SetActive(true);

			int orders = _currentSelectedQueue.QueueCount;

			CurrentProduction = (int)_currentSelectedQueue.CurrentProductionAmount;
			MaxProduction = _currentSelectedQueue.CurrentOrder.ProductionRequired;

			for (int i = 0; i < orders; i++) {
				if (i < _queueIcons.Length) {
					ISelectable selectable = GetSelectableFromProductionOrder(_currentSelectedQueue.Queue[i]);
					_queueIcons[i].sprite = selectable.Icon;
					_queueObjects[i].SetActive(true);
				}
				else {
					_queueObjects[_queueObjects.Length - 1].SetActive(false);
					_overflow.text = "+" + (orders - _queueObjects.Length);
					_overflow.transform.parent.gameObject.SetActive(true);
				}
			}

			if (orders < _queueObjects.Length) {
				for (int i = orders; i < _queueObjects.Length; i++) {
					_queueObjects[i].SetActive(false);
				}
			}
		}

		private static ISelectable GetSelectableFromProductionOrder(ProductionOrder order) {
			if (!Registry.Registry.TryGetPrefab(order.ProductKey, out GameObject gameObj)) {
				RatLogger.Error?.Log($"Error getting icon for prefab {order.ProductKey}, no registry entry found.");
			}
			return gameObj.GetComponent<ISelectable>();
		}
	}
}