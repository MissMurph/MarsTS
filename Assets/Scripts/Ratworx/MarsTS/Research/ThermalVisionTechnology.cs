using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Research
{
    public class ThermalVisionTechnology : Technology
    {
        [SerializeField] private NetworkObject _upgradePrefab;

        [SerializeField] private GameObject[] _applicableEntities;

        private HashSet<string> _applicableKeys;

        protected override void Start() {
            base.Start();

            if (!NetworkManager.Singleton.IsServer) return;

            _applicableKeys = new HashSet<string>();

            foreach (GameObject prefab in _applicableEntities) {
                Entity entity = prefab.GetComponent<Entity>();
                _applicableKeys.Add(entity.RegistryKey);
            }

            EventBus.AddListener<EntityInitEvent>(OnEntityInit);
        }

        private void OnEntityInit(EntityInitEvent evnt) {
            if (!_applicableKeys.Contains(evnt.Entity.RegistryKey))
                return;

            NetworkObject newObject = Instantiate(_upgradePrefab);
            newObject.Spawn();

            if (newObject.TrySetParent(evnt.Entity.gameObject)) return;
            
            RatLogger.Error?.Log($"Error parenting Technology {newObject.name} to owner {evnt.Entity.gameObject.name}");
            Destroy(newObject);
        }
    }
}