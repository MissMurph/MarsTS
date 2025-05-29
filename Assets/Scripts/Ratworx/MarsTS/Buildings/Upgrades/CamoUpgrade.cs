using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using UnityEngine;

namespace Ratworx.MarsTS.Buildings.Upgrades
{
    public class CamoUpgrade : MonoBehaviour
    {
        private void Start() {
            Entity parent = GetComponentInParent<Entity>();
            EventAgent eventAgent = GetComponentInParent<EventAgent>();
            eventAgent.PostLocal(new SneakEvent(parent, true));
        }
    }
}