using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Entities {

    public class EntityAttribute : MonoBehaviour, ITaggable<EntityAttribute> {

		public int Amount {
			get {
				return stored;
			}
		}

        [SerializeField]
        protected string key;

        [SerializeField]
        protected int startingValue;

		protected int stored;

		public string Key {
			get {
				return "attribute:" + key;
			}
		}

		public Type Type {
			get {
				return typeof(EntityAttribute);
			}
		}

		public EntityAttribute Get () {
			return this;
		}

		protected virtual void Awake () {
			stored = startingValue;
		}

		public virtual void Submit (int amount) {
			stored += amount;
		}

		public virtual bool Consume (int amount) {
			if (Amount >= amount) {
				stored -= amount;
				return true;
			}
			else return false;
		}
	}
}