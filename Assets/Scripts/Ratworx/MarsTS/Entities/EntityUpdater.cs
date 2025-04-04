using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Logging;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Entities {
    [RequireComponent(typeof(EntityCache))]
    public class EntityUpdater : MonoBehaviour {

        private EntityCache _entityCache;

        private bool _isUpdating;

        private bool _isClient;
        private bool _isServer;

        private void Awake() {
            _entityCache = GetComponent<EntityCache>();
        }

        private void Start() {
            _isServer = NetworkManager.Singleton.IsServer;
            _isClient = NetworkManager.Singleton.IsClient;
        }

        private void Update() {
            _isUpdating = true;
            
            using IEnumerator<Entity> updateCache = _entityCache.GetEnumerator();
            while (_isUpdating) {
                try {
                    while (updateCache.MoveNext() && updateCache.Current is not null) {
                        Entity entity = updateCache.Current;
                        if (_isServer) entity.ServerUpdate();
                        if (_isClient) entity.ClientUpdate();
                    }

                    _isUpdating = false;
                }
                catch (Exception exception) {
                    RatLogger.Error?.Log(exception);
                }
            }
        }
    }
}