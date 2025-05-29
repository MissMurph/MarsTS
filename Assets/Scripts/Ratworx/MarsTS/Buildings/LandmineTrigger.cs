using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS.Units.Turrets;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings
{
    public class LandmineTrigger : MonoBehaviour
    {
        private bool detonated = false;
        private UnitOwnership _ownership;

        private Explosion _explosion;
        private EventAgent _eventAgent;
        private Entity _entity;

        [SerializeField] private int damage;

        private void Awake() {
            _ownership = GetComponent<UnitOwnership>();
            _eventAgent = GetComponent<EventAgent>();
            _entity = GetComponent<Entity>();

            _explosion = transform.Find("Explosion").GetComponent<Explosion>();
            _explosion.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other) {
            if (detonated) return;

            if (!EntityCache.TryGetEntityComponent(other.transform.root.name, out IAttackable unit) ||
                !other.transform.CompareTag("Vehicle")) return;

            if (unit.GetRelationship(_ownership.Owner) == Teams.Relationship.Owned ||
                unit.GetRelationship(_ownership.Owner) == Teams.Relationship.Friendly) return;
            Detonate();
            detonated = true;
        }

        private void Detonate() {
            _explosion.Init(damage, _ownership.Owner, _entity);
            _explosion.gameObject.SetActive(true);
            _explosion.transform.SetParent(null, true);

            _eventAgent.PostGlobal(new UnitDeathEvent(_entity));

            Destroy(gameObject);
        }
    }
}