using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.WorldObject
{
    public class OilDeposit : ResourceDeposit
    {
        private GameObject _selectionCollider;
        private GameObject _resourceBars;

        public bool Exploited
        {
            get => _exploited;
            set
            {
                _exploited = value;
                _selectionCollider.SetActive(!_exploited);
                _resourceBars.SetActive(!_exploited);
            }
        }

        private bool _exploited;

        protected override void Awake()
        {
            base.Awake();
            _selectionCollider = transform.Find("SelectionCollider").gameObject;
            _resourceBars = transform.Find("BarOrientation").gameObject;
        }

        public override bool CanHarvest(string resourceKey, Entity unit)
            => resourceKey == "oil" && unit.RegistryKey.Contains("pumpjack");
    }
}