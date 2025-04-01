using Ratworx.MarsTS.Pathfinding;
using UnityEngine;

namespace Ratworx.MarsTS.Units {

    public class GroundDetection : MonoBehaviour {

		public bool Grounded { get; private set; }

		public RaycastHit Slope { get; private set; }

		private BoxCollider groundCollider;

		private Vector3 detectionPos;

		private void Awake () {
			groundCollider = transform.Find("GroundCollider").GetComponent<BoxCollider>();

			detectionPos = new Vector3(groundCollider.transform.localPosition.x, groundCollider.transform.localPosition.y - (groundCollider.bounds.size.y / 2), groundCollider.transform.localPosition.z);
		}

		private void Update () {
			Grounded = Physics.CheckBox(transform.TransformPoint(detectionPos),
				groundCollider.bounds.extents,
				groundCollider.transform.rotation,
				GameWorld.EnvironmentMask);

			Physics.Raycast(transform.position, transform.up * -1, out RaycastHit Slope, groundCollider.bounds.size.y, GameWorld.EnvironmentMask);
		}
	}
}