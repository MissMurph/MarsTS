using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Locomotion
{
    public class InfantryLocomotion : AbstractLocomotion
    {
        [SerializeField] private EntityAttribute _moveSpeedAttribute;

        public override void UpdatePhysics() {
            if (!Ground.Grounded) return;

            if (!Pathing.CurrentPath.IsEmpty) {
                Vector3 targetWaypoint = Pathing.CurrentWaypoint;

                Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0,
                    targetWaypoint.z - transform.position.z).normalized;
                float targetAngle = Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90f;
                RigidBody.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z));

                Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, Ground.Slope.normal);

                Vector3 newVelocity = moveDirection * _moveSpeedAttribute.Value;

                RigidBody.velocity = newVelocity;
            }
            else {
                RigidBody.velocity = Vector3.zero;
            }
        }
    }
}