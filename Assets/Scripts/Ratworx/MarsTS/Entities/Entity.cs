using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Registry;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Entities
{
    [RequireComponent(typeof(EventAgent))]
    [RequireComponent(typeof(NetworkObject))]
    public class Entity : NetworkBehaviour, 
                          IRegistryObject<Entity>,
                          IEquatable<Entity>
    {
        public int Id { get; private set; }

        public string RegistryKey => _registryKey;
        
        [FormerlySerializedAs("registryKey")] 
        [SerializeField] 
        private string _registryKey;

        public string RegistryType => _registryType;

        [SerializeField]
        private string _registryType;


        /// <summary>This safer init event will post immediately if the entity is already initialized</summary>
        public event Action<Phase> OnEntityInit
        {
            add
            {
                if (Id > 0)
                    value.Invoke(Phase.Post);
                else
                    _onEntityInitEvents += value;
            }
            remove => _onEntityInitEvents -= value;
        }

        private Action<Phase> _onEntityInitEvents;

        private Dictionary<string, IEntityComponent> _registeredEntityComponents;
        private Dictionary<string, Component> _taggedComponents;
        private List<IEntityServerUpdate> _serverUpdateComponents;
        private List<IEntityClientUpdate> _clientUpdateComponents;
        private List<IEntityPhysicsUpdate> _physicsUpdateComponents;

        private EventAgent _eventAgent;

        [FormerlySerializedAs("toTag")] [SerializeField] private TagReference[] _toTag;

        private void Awake()
        {
            _eventAgent = GetComponent<EventAgent>();

            _registeredEntityComponents = new Dictionary<string, IEntityComponent>();
            _taggedComponents = new Dictionary<string, Component>();

            foreach (IEntityComponent component in GetComponents<IEntityComponent>())
            {
                _registeredEntityComponents[component.Key] = component;
            }

            _serverUpdateComponents = GetComponentsInChildren<IEntityServerUpdate>().ToList();
            _clientUpdateComponents = GetComponentsInChildren<IEntityClientUpdate>().ToList();
            _physicsUpdateComponents = GetComponentsInChildren<IEntityPhysicsUpdate>().ToList();

            if (TryGetComponent(out NetworkObject found)) _taggedComponents["networking"] = found;

            foreach (TagReference entry in _toTag)
            {
                _taggedComponents[entry.Tag] = entry.Component;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            GameInit.OnSpawnEntities += Initialize;
        }

        internal void ServerUpdate() {
            foreach (IEntityServerUpdate component in _serverUpdateComponents) {
                if (component is MonoBehaviour { enabled: false }) 
                    return;
                
                component.UpdateServer();
            }
        }

        internal void ClientUpdate() {
            foreach (IEntityClientUpdate component in _clientUpdateComponents) {
                if (component is MonoBehaviour { enabled: false }) 
                    return;
                
                component.UpdateClient();
            }
        }
        
        internal void PhysicsUpdate() {
            foreach (IEntityPhysicsUpdate component in _physicsUpdateComponents) {
                if (component is MonoBehaviour { enabled: false }) 
                    return;
                
                component.UpdatePhysics();
            }
        }

        private void Initialize()
        {
            Id = EntityCache.Register(this);
            name = $"{_registryKey}:{Id}";

            //GetComponent<NetworkObject>().Spawn();
            SynchronizeClientRpc(Id);
            PostInitEvents();
        }

        [Rpc(SendTo.NotServer)]
        private void SynchronizeClientRpc(int id)
        {
            Id = id;
            name = $"{_registryKey}:{Id}";
            EntityCache.Register(this);
            PostInitEvents();
        }

        private void PostInitEvents()
        {
            EntityInitEvent initCall = new EntityInitEvent(this, _eventAgent);

            // Broken up into two steps for silly business, I think, I don't quite remember lmao
            initCall.Phase = Phase.Pre;
            _onEntityInitEvents?.Invoke(Phase.Pre);
            _eventAgent.PostGlobal(initCall);

            initCall.Phase = Phase.Post;
            _onEntityInitEvents?.Invoke(Phase.Post);
            _eventAgent.PostGlobal(initCall);
        }

        public bool TryGetEntityComponent<T>(string key, out T output)
        {
            if (_registeredEntityComponents.TryGetValue(key, out IEntityComponent component) && component is T superType)
            {
                output = superType;
                return true;
            }
            
            if (typeof(T) == typeof(Component) && _taggedComponents.TryGetValue(key, out Component found) &&
                found is T superTypedComponent)
            {
                output = superTypedComponent;
                return true;
            }

            output = default;
            return false;
        }

        public bool TryGetEntityComponent<T>(out T output)
        {
            foreach (IEntityComponent taggableComponent in _registeredEntityComponents.Values)
            {
                if (taggableComponent is T superType)
                {
                    output = superType;
                    return true;
                }
            }

            if (typeof(Component).IsAssignableFrom(typeof(T)))
                foreach (Component nonTaggableComponent in _taggedComponents.Values)
                {
                    if (nonTaggableComponent is T superTypedComponent)
                    {
                        output = superTypedComponent;
                        return true;
                    }
                }

            output = default;
            return false;
        }

        public T GetEntityComponent<T>(string key)
        {
            if (_registeredEntityComponents.TryGetValue(key, out IEntityComponent taggable) && taggable is T superType)
                return superType;

            if (typeof(T).IsSubclassOf(typeof(Component))
                && _taggedComponents.TryGetValue(key, out Component component)
                && component is T superTypedComponent)
                return superTypedComponent;

            return default;
        }

        public override void OnDestroy()
        {
            _eventAgent.PostGlobal(new EntityDestroyEvent(this));
        }

        public Entity GetEntityComponent() => this;

        public bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Entity)obj);
        }

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Id);
    }

    [Serializable]
    public class TagReference
    {
        public string Tag;
        public Component Component;
    }
}