using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
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

        protected override void Start()
        {
            base.Start();
            
            if (!NetworkManager.Singleton.IsServer) return;
            
            _applicableKeys = new HashSet<string>();

            foreach (GameObject entity in _applicableEntities)
            {
                if (!entity.TryGetComponent(out ISelectable unit)) continue;

                _applicableKeys.Add(unit.RegistryKey);
            }
            
            EventBus.AddListener<EntityInitEvent>(OnEntityInit);
        }

        private void OnEntityInit(EntityInitEvent evnt)
        {
            if (!evnt.ParentEntity.TryGetEntityComponent(out ISelectable unit)
                || !_applicableKeys.Contains(unit.RegistryKey))
                return;

            NetworkObject newObject = Instantiate(_upgradePrefab);
            newObject.Spawn();

            if (!newObject.TrySetParent(unit.GameObject))
            {
                Debug.LogError($"Error parenting Technology {newObject.name} to owner {unit.GameObject.name}");
                Destroy(newObject);
            }
        }
    }
}