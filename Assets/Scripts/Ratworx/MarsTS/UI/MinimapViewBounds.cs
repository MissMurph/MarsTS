using UnityEngine;

namespace Ratworx.MarsTS.UI {

    public class MinimapViewBounds : MonoBehaviour {

		//LineRenderer aligns sprite to attached camera, so the renderer component needs to be attached
		//to the mini map camera in order to align the sprite with that camera specifically
		//This means we need to convert local positions back to global when setting
		private LineRenderer lineRenderer;

		private Vector3[] corners;

		private Plane groundPlane;

		private void Awake () {
			//lineRenderer = GetComponent<LineRenderer>();
			corners = new Vector3[4];

			//At -1 cus the world is weirdly at -1
			groundPlane = new Plane(Vector3.up, new Vector3(0, -1, 0));
		}

		private void Start () {
			lineRenderer = GameObject.FindGameObjectWithTag("ViewportRenderer").GetComponent<LineRenderer>();
		}

		private void Update () {
			UpdateCorners();

			for (int i = 0; i < corners.Length; i++) {
				lineRenderer.SetPosition(i, transform.TransformPoint(corners[i]));
			}

			//lineRenderer.SetPositions(corners);
		}

		private void UpdateCorners () {
			corners[0] = GetCorner(Vector2.up, 0);
			corners[1] = GetCorner(Vector2.one, 1);
			corners[2] = GetCorner(Vector2.right, 2);
			corners[3] = GetCorner(Vector2.zero, 3);
		}

		private Vector3 GetCorner (Vector2 position, int index) {
			Ray ray = Player.Player.ViewPort.ViewportPointToRay(position);
			Vector3 output = new Vector3();

			if (groundPlane.Raycast(ray, out float distance)) {
				//By storing these as local positions, when the camera moves, even if out of the bounds
				//The points still move with the camera, fixing the NAN errors
				output = transform.InverseTransformPoint(ray.GetPoint(distance));
			}
			else {
				output = corners[index];
			}

			//Debug.DrawLine(ray.origin, ray.GetPoint(distance), Color.cyan, 1f);

			return output;
		}
	}
}