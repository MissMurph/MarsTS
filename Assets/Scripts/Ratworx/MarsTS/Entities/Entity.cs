using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS;
using Ratworx.MarsTS.Registry;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Entities
{
    [RequireComponent(typeof(EventAgent))]
    public class Entity : NetworkBehaviour, IRegistryObject<Entity>
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
            
        }

        internal void ClientUpdate() {
            
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
            _eventAgent.Global(initCall);

            initCall.Phase = Phase.Post;
            _onEntityInitEvents?.Invoke(Phase.Post);
            _eventAgent.Global(initCall);
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
            _eventAgent.Global(new EntityDestroyEvent(_eventAgent, this));
        }

        public Entity GetEntityComponent() => this;
    }

    [Serializable]
    public class TagReference
    {
        public string Tag;
        public Component Component;
    }
}