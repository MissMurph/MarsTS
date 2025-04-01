using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Entities {

	//This is a seperate class due to the significance of Health, it operates differently
	//To other attributes so making it inherit creates a lot of spaghetti
    public class HealthAttribute : MonoBehaviour, ITaggable<HealthAttribute> {
		
		public int Health {
			get {
				return health;
			}
		}

		public int MaxHealth {
			get {
				return maxHealth;
			}
		}

		[SerializeField]
		private int startingValue;

		private int health;

		[SerializeField]
		private int maxHealth;

		public string Key {
			get {
				return "health";
			}
		}

		public Type Type {
			get {
				return typeof(HealthAttribute);
			}
		}

		public HealthAttribute Get () {
			return this;
		}

		private void Awake () {
			health = startingValue;
		}

		
	}
}