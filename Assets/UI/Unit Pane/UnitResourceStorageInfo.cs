using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Units;
using TMPro;
using UnityEngine;

namespace MarsTS.UI
{
    public class UnitResourceStorageInfo : MonoBehaviour, IInfoModule
    {
        private ResourceStorage _trackedStorage;
        private int _maxValue;
        private int _currentValue;
        private float _fillLevel;

        public GameObject GameObject => gameObject;

        public string Name => _name;

        [SerializeField] private string _name;

        private TextMeshProUGUI _text;
        private RectTransform _barTransform;

        private float _literalSize;

        private void Awake()
        {
            _text = transform.Find("Number").GetComponent<TextMeshProUGUI>();
            _barTransform = transform.Find("Bar") as RectTransform;

            //xMax is the max literal x co-ords from the center, so if we multiply by 2 that gets us the literal size
            _literalSize = _barTransform.rect.xMax * 2;
        }

        private void OnStorageValueChanged(int oldAmount, int newAmount)
        {
            UpdateStoredAmount(newAmount, _maxValue);
        }

        public void SetStorage(ResourceStorage storage)
        {
            _trackedStorage = storage;

            UpdateStoredAmount(storage.Amount, storage.Capacity);

            storage.OnAttributeChange += OnStorageValueChanged;

            if (!EntityCache.TryGet(storage.transform.root.name, out EventAgent storageEvents))
                Debug.LogWarning(
                    $"Couldn't get {typeof(EventAgent)} from Unit {storage.transform.root.name} on Info module {typeof(UnitResourceStorageInfo)}!");
            else
                storageEvents.AddListener<UnitDeathEvent>(OnEntityDeath);
        }

        private void UpdateStoredAmount(int currentValue, int maxValue)
        {
            _maxValue = maxValue;
            _currentValue = currentValue;
            _fillLevel = (float)_currentValue / _maxValue;

            float rightEdge = _literalSize - _literalSize * _fillLevel;
            _barTransform.offsetMax = new Vector2(-rightEdge, 0f);

            _text.text = $"{_currentValue} / {_maxValue}";
        }

        private void OnEntityDeath(UnitDeathEvent _event)
        {
            Deactivate();
        }

        public T Get<T>()
        {
            if (this is T output) return output;
            return default;
        }

        public void Deactivate()
        {
            if (_trackedStorage != null)
            {
                if (!EntityCache.TryGet(_trackedStorage.transform.root.name, out EventAgent storageEvents))
                    Debug.LogWarning(
                        $"Couldn't get {typeof(EventAgent)} from Unit {_trackedStorage.transform.root.name} on Info module {typeof(UnitResourceStorageInfo)}!");
                else
                    storageEvents.RemoveListener<UnitDeathEvent>(OnEntityDeath);

                _trackedStorage.OnAttributeChange -= OnStorageValueChanged;
                _trackedStorage = null;
            }

            gameObject.SetActive(false);
        }
    }
}