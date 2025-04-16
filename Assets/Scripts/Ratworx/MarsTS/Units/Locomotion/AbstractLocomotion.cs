using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units.Locomotion
{
    public abstract class AbstractLocomotion : MonoBehaviour,
                                      IEntityComponent<AbstractLocomotion>,
                                      IEntityPhysicsUpdate,
                                      IUnitInterface
    {
        public GameObject GameObject => gameObject;
        public Entity Entity => _entity;
        public AbstractLocomotion Get() => this;
        public string Key => "locomotion";
        
        private Entity _entity;
        
        protected Rigidbody RigidBody;
        protected UnitPathfinder Pathing;
        protected GroundDetection Ground;
        
        private void Awake() {
            _entity = GetComponent<Entity>();
            RigidBody = GetComponent<Rigidbody>();
            Pathing = GetComponent<UnitPathfinder>();
            Ground = GetComponent<GroundDetection>();
        }

        public abstract void UpdatePhysics();
    }
}