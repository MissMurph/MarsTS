using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using UnityEngine;

namespace Ratworx.MarsTS.Units.SafeReference
{
    // TODO: Make T : IUnit so we can imply gameobject from it
    public class UnitReference<T> where T : class
    {
        public T Get { get; private set; }

        public GameObject GameObject { get; private set; }

        public void Set(T newValue, GameObject _unit)
        {
            if (Get != null)
            {
                EntityCache.TryGetEntityComponent(GameObject.name + ":eventAgent", out EventAgent oldAgent);
                oldAgent.RemoveListener<UnitDeathEvent>(OnEntityDeath);
                oldAgent.RemoveListener<EntityVisibleEvent>(OnEntityVisible);
            }

            Get = newValue;
            GameObject = _unit;

            if (Get != null)
            {
                EntityCache.TryGetEntityComponent(_unit.name + ":eventAgent", out EventAgent agent);
                agent.AddListener<UnitDeathEvent>(OnEntityDeath);
                agent.AddListener<EntityVisibleEvent>(OnEntityVisible);
            }
        }

        private void OnEntityDeath(UnitDeathEvent _event)
        {
            Set(null, null);
        }

        private void OnEntityVisible(EntityVisibleEvent _event)
        {
            if (!_event.Visible) Set(null, null);
        }
    }
}