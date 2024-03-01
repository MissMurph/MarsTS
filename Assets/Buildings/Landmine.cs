using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class Landmine : Building {

		protected void OnTriggerEnter (Collider other) {
			if (!Constructed) return;
		}
	}
}