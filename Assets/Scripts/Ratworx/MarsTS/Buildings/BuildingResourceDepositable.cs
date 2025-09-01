using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings
{
    public class BuildingResourceDepositable : MonoBehaviour,
                                           IDepositable,
                                           IEntityComponent<IDepositable>
    {
        public GameObject GameObject => gameObject;
        public Entity Entity { get; private set; }

        public IDepositable Get() => this;
        public string Key => "depositable";

        private UnitOwnership _ownership;

        private void Awake() {
            Entity = GetComponentInParent<Entity>();
            _ownership = GetComponentInParent<UnitOwnership>();
        }

        public int Deposit(string resourceKey, int depositAmount)
            => _ownership.Owner.GetResource(resourceKey).Deposit(depositAmount) ? depositAmount : 0;
    }
}