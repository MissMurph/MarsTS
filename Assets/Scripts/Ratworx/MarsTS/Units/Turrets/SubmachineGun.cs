using UnityEngine;

namespace Ratworx.MarsTS.Units.Turrets
{
    public class SubmachineGun : ProjectileTurret
    {
        // Amount of bullets fired per burst
        [SerializeField] private int _burstCount;
        // Time in seconds between bullets in burst
        [SerializeField] private float _burstCooldown;
        
        private int _firedCount;
        private float _currentBurstCooldown;

        public override void UpdateServer() {
            if (CurrentCooldown > 0f) CurrentCooldown -= Time.deltaTime;
            if (_burstCooldown > 0f) _currentBurstCooldown -= Time.deltaTime;
            
            if (TrackedTarget == null) return;
            if (CurrentCooldown > 0f) return;
            if (_currentBurstCooldown > 0f) return;
            
            FireProjectile(TrackedTarget.GameObject.transform.position);
            _firedCount++;
            _currentBurstCooldown += _burstCooldown;

            if (_firedCount >= _burstCount) 
                CurrentCooldown += Cooldown;
        }
    }
}