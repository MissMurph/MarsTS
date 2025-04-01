using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class ResourceStorage : EntityAttribute
    {
        public int Capacity => _capacity;
        public string Resource => _resourceKey;
        [SerializeField] private int _capacity;
        [SerializeField] private string _resourceKey;

        private void Awake()
        {
            _key = "storage:" + _resourceKey;
        }

        public override int Submit(int amount)
        {
            int newAmount = Mathf.Min(_capacity, Amount + amount);

            int difference = newAmount - Amount;

            Amount = newAmount;

            return difference;
        }
    }
}