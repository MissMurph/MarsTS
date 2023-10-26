using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class Agent : MonoBehaviour {

		//This will return 0 if the agent isn't registered
		public int Id {
			get {
				return id;
			}
		}

		private int id = 0;

		private void Start () {
			EventBus.RegisterAgent(Initialize, this);
		}

		private void Initialize (int _id) {
			if (id == 0) {
				id = _id;
			}
		}
	}
}