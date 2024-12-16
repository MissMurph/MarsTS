using System;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Entities
{
    public class EntityAttribute : MonoBehaviour, ITaggable<EntityAttribute>
    {
        public event Action<int, int> AttributeChangeEvent;
        
        public virtual int Amount
        {
            get => _stored.Value;
            set => _stored.Value = value < 0 ? 0 : value;
        }

        [SerializeField] protected string _key;

        [SerializeField] protected int _startingValue;

        protected NetworkVariable<int> _stored =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        public string Key => "attribute:" + _key;

        public Type Type => typeof(EntityAttribute);

        public EntityAttribute Get() => this;

        protected virtual void Awake()
        {
            _stored.OnValueChanged += (oldValue, newValue) => AttributeChangeEvent?.Invoke(oldValue, newValue);
            Amount = _startingValue;
        }

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