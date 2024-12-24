using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.Units;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Editor
{
    public class EntitySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab = null;
        [SerializeField] private int _owner;
        // Tells the spawner to wait until manually called if true
        [SerializeField] private bool _deferSpawn;

        private GameObject _model;

        private MeshFilter[] _meshRenderers;

        private void Awake()
        {
            EventBus.AddListener<PlayerInitEvent>(OnPlayerInit);
        }

        /// <summary>Will tell the spawner to wait until <see cref="SpawnEntity"/> is called before spawning.</summary>
        public void SetDeferredSpawn(bool value) => _deferSpawn = value;

        public void SetOwner(int newValue) => _owner = newValue;

        public void SpawnEntity()
        {
            InstantiateAndSpawnEntity();
        }

        private void OnPlayerInit(PlayerInitEvent _event)
        {
            if (_deferSpawn || !NetworkManager.Singleton.IsServer) return;

            InstantiateAndSpawnEntity();
        }

        private void InstantiateAndSpawnEntity()
        {
            GameObject instantiated = Instantiate(_prefab, transform.position, transform.rotation);
            var selectable = instantiated.GetComponent<ISelectable>();
            var networkObject = instantiated.GetComponent<NetworkObject>();

            networkObject.Spawn();
            
            if (_owner > 0) 
                selectable.SetOwner(TeamCache.Faction(_owner));
            
            Destroy(gameObject);
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