using System;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Entities
{
    public class EntityAttribute : NetworkBehaviour, ITaggable<EntityAttribute>
    {
        public event Action<int, int> OnAttributeChange;
        
        public virtual int Amount
        {
            get => _stored.Value;
            protected set => _stored.Value = value < 0 ? 0 : value;
        }

        [SerializeField] protected string _key;

        [SerializeField] protected int _startingValue;

        protected NetworkVariable<int> _stored =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        public string Key => "attribute:" + _key;

        public Type Type => typeof(EntityAttribute);

        public EntityAttribute Get() => this;

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsServer) 
                Amount = _startingValue;
            
            if (NetworkManager.Singleton.IsClient) 
                _stored.OnValueChanged += OnStoredValueChange;
        }

        private void OnStoredValueChange(int oldValue, int newValue) => OnAttributeChange?.Invoke(oldValue, newValue);

        public virtual int Submit(int amount)
        {
            Amount += amount;
            return amount;
        }

        public virtual bool Consume(int amount)
        {
            if (Amount < amount) 
                return false;
            
            Amount -= amount;
            return true;
        }
    }
}