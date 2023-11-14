using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MarsTS.UI {

    public abstract class UnitBar : MonoBehaviour {

		protected MeshRenderer barRenderer;

		private MaterialPropertyBlock matBlock;

		protected float FillLevel {
			get {
				return fill;
			}
			set {
				fill = value;

				barRenderer.GetPropertyBlock(matBlock);
				matBlock.SetFloat("_Fill", fill);
				barRenderer.SetPropertyBlock(matBlock);
			}
		}

		private float fill;

		protected virtual void Awake () {
			barRenderer = GetComponent<MeshRenderer>();
			matBlock = new MaterialPropertyBlock();
		}

		protected virtual void Update () {
			Transform camera = Camera.main.transform;
			Vector3 direction = transform.position - camera.position;

			direction.Normalize();

			Vector3 up = Vector3.Cross(direction, camera.right);

			transform.rotation = Quaternion.LookRotation(direction, up);
		}
	}
}