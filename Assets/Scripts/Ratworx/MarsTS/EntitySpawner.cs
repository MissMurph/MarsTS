using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using Ratworx.MarsTS;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS
{
    public class EntitySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab = null;
        [SerializeField] private int _owner;
        // Tells the spawner to wait until manually called if true
        [SerializeField] private bool _deferSpawn;
        [SerializeField] private bool _destroyOnSpawn = true;

        private GameObject _model;

        private MeshFilter[] _meshRenderers;

        private void Start()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Destroy(gameObject);
                return;
            }
            
            GameInit.OnSpawnEntities += OnPlayerInit;
            //EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
        }

        /// <summary>Will tell the spawner to wait until <see cref="SpawnEntity"/> is called before spawning.</summary>
        public void SetDeferredSpawn(bool value) => _deferSpawn = value;

        public void SetOwner(int newValue) => _owner = newValue;

        public void SetEntity(GameObject prefab) => _prefab = prefab;

        public void SetDestroyOnSpawn(bool value) => _destroyOnSpawn = value;

        public Entity SpawnEntity()
        {
            return InstantiateAndSpawnEntity();
        }

        private void OnPlayerInit()
        {
            if (_deferSpawn) return;

            InstantiateAndSpawnEntity();
        }

        private Entity InstantiateAndSpawnEntity()
        {
            RatLogger.Verbose?.Log($"Spawning {_prefab.name}");
            
            GameObject instantiated = Instantiate(_prefab, transform.position, transform.rotation);
            var selectable = instantiated.GetComponent<ISelectable>();
            var networkObject = instantiated.GetComponent<NetworkObject>();
            var entity = instantiated.GetComponent<Entity>();

            networkObject.Spawn();
            
            if (_owner > 0) 
                selectable.SetOwner(TeamCache.Faction(_owner));
            
            if (_destroyOnSpawn) 
                Destroy(gameObject, 0.1f);

            return entity;
        }

        private void OnValidate()
        {
            if (!_prefab) return;
            
            Transform model = _prefab.transform.Find("Model");

            _meshRenderers = model.GetComponentsInChildren<MeshFilter>();
        }

        private void OnDrawGizmos()
        {
            if (_prefab is null || _meshRenderers is null || _meshRenderers.Length <= 0) return;

            Transform baseTransform = transform;

            foreach (var model in _meshRenderers)
            {
                var position = transform.TransformPoint(model.transform.position);
                var rotation = model.transform.rotation * transform.rotation;
                var scale = model.transform.localScale;
                
                Gizmos.color = Color.cyan;
                Gizmos.DrawMesh(model.sharedMesh, position, rotation, scale);
            }
        }
    }
}