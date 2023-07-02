using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units.Attacks {

	public class Attack : MonoBehaviour {

		[SerializeField]
		private SphereCollider Range;

		[SerializeField]
		private int damage;

		[SerializeField]
		private float fireRate;

		private void Update () {
			
		}
	}

}