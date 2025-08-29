using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class BarOrientation : MonoBehaviour,
                                  IEntityClientUpdate
    {
        public void UpdateClient() {
            if (Camera.main is null) return;

            Transform camera = Camera.main.transform;

            /*Vector3 direction = transform.position - camera.position;

            direction.Normalize();

            Vector3 up = Vector3.Cross(direction, camera.right);

            transform.rotation = Quaternion.LookRotation(direction, up);*/

            transform.rotation = Quaternion.Euler(0, camera.transform.rotation.eulerAngles.y, 0);
        }
    }
}