using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Buildings {

    public class BuildingGhost : MonoBehaviour {

		protected List<Collider> collisions = new List<Collider>();

		[SerializeField]
		protected Material legalMat;

		[SerializeField]
		protected Material illegalMat;

		private Renderer[] allRenderers;

		public virtual bool Legal {
			get {
				return collisions.Count == 0;
			}
		}

		private void Awake () {
			allRenderers = GetComponentsInChildren<Renderer>();
		}

		private void Update () {
			if (Legal) {
				foreach (Renderer render in allRenderers) {
					render.material = legalMat;
				}
			}
			else {
				foreach (Renderer render in allRenderers) {
					render.material = illegalMat;
				}
			}
		}

		protected virtual void OnTriggerEnter (Collider other) {
			if (!collisions.Contains(other)) {
				collisions.Add(other);
			}
        }

		protected virtual void OnTriggerExit (Collider other) {
			if (collisions.Contains(other)) {
				collisions.Remove(other);
			}
		}
	}
}