using System;
using UnityEngine;

namespace MarsTS.Entities
{
    public class EntityAttribute : MonoBehaviour, ITaggable<EntityAttribute>
    {
        public virtual int Amount
        {
            get => stored;
            set
            {
                stored = value;
                if (stored < 0) stored = 0;
            }
        }

        [SerializeField] protected string key;

        [SerializeField] protected int startingValue;

        protected int stored;

        public string Key => "attribute:" + key;

        public Type Type => typeof(EntityAttribute);

        public EntityAttribute Get() => this;

        protected virtual void Awake()
        {
            stored = startingValue;
        }

        public virtual int Submit(int amount)
        {
            stored += amount;
            return amount;
        }

        public virtual bool Consume(int amount)
        {
            if (Amount >= amount)
            {
                stored -= amount;
                return true;
            }

            return false;
        }
    }
}