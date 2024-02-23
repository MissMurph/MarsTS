using MarsTS.Players;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class MinimapViewBounds : MonoBehaviour {

		private LineRenderer lineRenderer;

		[SerializeField]
		private Vector3[] corners;

		private Plane groundPlane;

		private void Awake () {
			lineRenderer = GetComponent<LineRenderer>();
			corners = new Vector3[4];

			//At -1 cus the world is weirdly at -1
			groundPlane = new Plane(Vector3.up, new Vector3(0, -1, 0));
		}

		private void Update () {
			UpdateCorners();

			lineRenderer.SetPositions(corners);
		}

		private void UpdateCorners () {
			corners[0] = GetCorner(Vector2.up);
			corners[1] = GetCorner(Vector2.one);
			corners[2] = GetCorner(Vector2.right);
			corners[3] = GetCorner(Vector2.zero);
		}

		private Vector3 GetCorner (Vector2 position) {
			Ray ray = Player.ViewPort.ViewportPointToRay(position);

			if (groundPlane.Raycast(ray, out float distance)) {
				return ray.GetPoint(distance);
			}
			else {
				return new Vector3();
			}

			//Debug.DrawLine(ray.origin, ray.GetPoint(distance), Color.cyan, 1f);

			
		}
	}
}