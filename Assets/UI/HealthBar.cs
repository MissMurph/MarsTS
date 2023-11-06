using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class HealthBar : MonoBehaviour {

        [SerializeField]
        private Collider unitCollider;

		private MeshRenderer barRenderer;

		private Vector3 size;

		private MaterialPropertyBlock matBlock;

		private void Awake () {
			barRenderer = GetComponent<MeshRenderer>();
			GetComponentInParent<EventAgent>().AddListener<EntityHurtEvent>(OnEntityHurt);

			barRenderer.enabled = false;

			size = unitCollider.bounds.size;
			transform.localPosition = new Vector3(0, size.y + 1f, 0);

			matBlock = new MaterialPropertyBlock();
		}

		private void Update () {
			Transform camera = Camera.main.transform;
			Vector3 direction = transform.position - camera.position;

			direction.Normalize();

			Vector3 up = Vector3.Cross(direction, camera.right);

			transform.rotation = Quaternion.LookRotation(direction, up);
		}

		private void OnEntityHurt (EntityHurtEvent _event) {
			if (!barRenderer.enabled) barRenderer.enabled = true;

			barRenderer.GetPropertyBlock(matBlock);

			matBlock.SetFloat("_Fill", (float)_event.Unit.Health / _event.Unit.MaxHealth);

			barRenderer.SetPropertyBlock(matBlock);
		}
	}
}