using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Entities {

    public class EntityAttribute : MonoBehaviour, ITaggable<EntityAttribute> {

		public virtual int Amount {
			get {
				return stored;
			}
			set {
				stored = value;
				if (stored < 0) stored = 0;
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

		public virtual int Submit (int amount) {
			stored += amount;
			return amount;
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