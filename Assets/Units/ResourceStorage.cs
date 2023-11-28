using MarsTS.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class ResourceStorage : EntityAttribute {

		public int Capacity { get { return capacity; } }

		public string Resource { get { return resourceKey; } }

        [SerializeField]
        private int capacity;

		[SerializeField]
		private string resourceKey;

		protected override void Awake () {
			base.Awake();

			key = "storage:" + resourceKey;
		}

		public override int Submit (int amount) {
			int newAmount = Mathf.Min(capacity, stored + amount);

			int difference = newAmount - stored;

			stored = newAmount;

			return difference;
		}
	}
}