using UnityEngine;

namespace Ratworx.MarsTS.Units.Locomotion
{
    public class CarLocomotion : AbstractLocomotion
    {
        [SerializeField] private float _topSpeed;
        [SerializeField] private float _reverseSpeed;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _currentSpeed;
        [SerializeField] private float _velocity;
        [SerializeField] private float _turnSpeed;

        [Header("Braking")]
        [SerializeField] private float _brakeWindowTime;
        [SerializeField] private float _brakeForce;
        
        private float CurrentAngle {
            get {
                float angle = transform.rotation.eulerAngles.y;
                return angle;
            }
        }

        public override void UpdatePhysics() {
            if (!Ground.Grounded) return;
            
            _velocity = RigidBody.velocity.sqrMagnitude;

            if (!Pathing.CurrentPath.IsEmpty) {
                Vector3 targetWaypoint = Pathing.CurrentWaypoint;

                Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;

                float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

                float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, _turnSpeed * Time.fixedDeltaTime);

                Vector3 currentVelocity = RigidBody.velocity;

                //float brakeThreshold = currentVelocity.magnitude * brakeWindowTime;

                RigidBody.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

                Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, Ground.Slope.normal);

                adjustedVelocity *= currentVelocity.magnitude;

                float accelCap = 1f - (_velocity / (_topSpeed * _topSpeed));

                RigidBody.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (_turnSpeed * accelCap) * Time.fixedDeltaTime);

                //Relative so it can take into account the forward vector of the car
                RigidBody.AddRelativeForce(Vector3.forward * (_acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);

                if (_velocity > _topSpeed * _topSpeed) {
                    Vector3 direction = RigidBody.velocity.normalized;
                    direction *= _topSpeed;
                    RigidBody.velocity = direction;
                }
            }
            else if (RigidBody.velocity.magnitude >= 0.5f) {
                RigidBody.AddRelativeForce(-RigidBody.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }
}