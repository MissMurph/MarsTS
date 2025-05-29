using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings
{
    public class PumpjackOilDetector : MonoBehaviour
    {
        private EventAgent _eventAgent;
        private OilDeposit _exploitedDeposit;

        private void Awake() {
            _eventAgent = GetComponentInParent<EventAgent>();
        }

        private void Start() {
            _eventAgent.AddListener<UnitDeathEvent>(OnBuildingDeath);
        }

        private void OnTriggerEnter(Collider other) {
            if (!EntityCache.TryGetEntityComponent(other.transform.root.name, out OilDeposit found)) 
                return;
            
            _exploitedDeposit = found;
            found.Exploited = true;
        }

        private void OnBuildingDeath(UnitDeathEvent evnt) {
            _exploitedDeposit.Exploited = false;
        }
    }
}