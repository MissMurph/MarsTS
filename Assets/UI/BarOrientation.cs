using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class BarOrientation : MonoBehaviour {

		private void Update () {
			Transform camera = Camera.main.transform;

			/*Vector3 direction = transform.position - camera.position;

			direction.Normalize();

			Vector3 up = Vector3.Cross(direction, camera.right);

			transform.rotation = Quaternion.LookRotation(direction, up);*/

			transform.rotation = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0);
		}
	}
}