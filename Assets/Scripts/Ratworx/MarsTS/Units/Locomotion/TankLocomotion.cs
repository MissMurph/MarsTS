using UnityEngine;

namespace Ratworx.MarsTS.Units.Locomotion
{
    public class TankLocomotion : AbstractLocomotion
    {
        [SerializeField] private float _topSpeed;
        [SerializeField] private float _currentTopSpeed;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _turnSpeed;
        [SerializeField] private float _angleTolerance;
        
        private float _velocity;
        
        private float CurrentAngle {
            get {
                float angle = transform.rotation.eulerAngles.y;
                return angle;
            }
        }
        
        public override void UpdatePhysics() {
            _velocity = RigidBody.velocity.sqrMagnitude;
            
            if (!Ground.Grounded) return;

            if (!Pathing.CurrentPath.IsEmpty) {
                Vector3 targetWaypoint = Pathing.CurrentWaypoint;

                Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;
                float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

                float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, _turnSpeed * Time.fixedDeltaTime);
                RigidBody.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

                Vector3 currentVelocity = RigidBody.velocity;
                Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, Ground.Slope.normal);

                adjustedVelocity *= currentVelocity.magnitude;

                if (Vector3.Angle(targetDirection, transform.forward) <= _angleTolerance) {
                    float accelCap = 1f - (_velocity / (_currentTopSpeed * _currentTopSpeed));

                    //This moves the velocity according to the rotation of the unit
                    RigidBody.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (_turnSpeed * accelCap) * Time.fixedDeltaTime);

                    //Relative so it can take into account the forward vector of the car
                    RigidBody.AddRelativeForce(Vector3.forward * (_acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);
                }

                if (!(_velocity > _currentTopSpeed * _currentTopSpeed)) return;
                
                Vector3 direction = RigidBody.velocity.normalized;
                direction *= _currentTopSpeed;
                RigidBody.velocity = direction;
            }
            else if (_velocity >= 0.5f) {
                RigidBody.AddRelativeForce(-RigidBody.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }
}