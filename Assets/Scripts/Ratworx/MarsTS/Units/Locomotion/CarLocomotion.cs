using System;
using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class CarLocomotion : MonoBehaviour,
                                 IEntityComponent<CarLocomotion>,
                                 IEntityPhysicsUpdate,
                                 IUnitInterface
    {
        public GameObject GameObject => gameObject;
        public Entity Entity => _entity;
        public CarLocomotion Get() => this;
        public string Key => "locomotion";
        
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

        private Entity _entity;
        private Rigidbody _rigidBody;
        private UnitPathfinder _pathing;
        private GroundDetection _ground;

        private void Awake() {
            _entity = GetComponent<Entity>();
            _rigidBody = GetComponent<Rigidbody>();
            _pathing = GetComponent<UnitPathfinder>();
            _ground = GetComponent<GroundDetection>();
        }

        public void UpdatePhysics() {
            if (!_ground.Grounded) return;
            
            _velocity = _rigidBody.velocity.sqrMagnitude;

            if (!_pathing.CurrentPath.IsEmpty) {
                Vector3 targetWaypoint = _pathing.CurrentWaypoint;

                Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0, targetWaypoint.z - transform.position.z).normalized;

                float targetAngle = (Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg) + 90f;

                float newAngle = Mathf.MoveTowardsAngle(CurrentAngle, targetAngle, _turnSpeed * Time.fixedDeltaTime);

                Vector3 currentVelocity = _rigidBody.velocity;

                //float brakeThreshold = currentVelocity.magnitude * brakeWindowTime;

                _rigidBody.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, newAngle, transform.eulerAngles.z));

                Vector3 adjustedVelocity = Vector3.ProjectOnPlane(transform.forward, _ground.Slope.normal);

                adjustedVelocity *= currentVelocity.magnitude;

                float accelCap = 1f - (_velocity / (_topSpeed * _topSpeed));

                _rigidBody.velocity = Vector3.Lerp(currentVelocity, adjustedVelocity, (_turnSpeed * accelCap) * Time.fixedDeltaTime);

                //Relative so it can take into account the forward vector of the car
                _rigidBody.AddRelativeForce(Vector3.forward * (_acceleration * accelCap) * Time.fixedDeltaTime, ForceMode.Acceleration);

                if (_velocity > _topSpeed * _topSpeed) {
                    Vector3 direction = _rigidBody.velocity.normalized;
                    direction *= _topSpeed;
                    _rigidBody.velocity = direction;
                }
            }
            else if (_rigidBody.velocity.magnitude >= 0.5f) {
                _rigidBody.AddRelativeForce(-_rigidBody.velocity * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }
    }
}