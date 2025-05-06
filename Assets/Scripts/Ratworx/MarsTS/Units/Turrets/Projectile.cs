using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Turrets {

    public class Projectile : MonoBehaviour, 
							  IEntityServerUpdate,
							  IEntityClientUpdate
	{
        [FormerlySerializedAs("speed")]
		[SerializeField]
        private float _speed;

		[FormerlySerializedAs("lifeTime")]
		[SerializeField]
		private float _lifeTime;

		private bool _initialized = false;

		private Faction _owner;

		private Action<bool, IAttackable> _hitCallback;

		public void Init (Faction owner, Action<bool, IAttackable> callback) {
			_owner = owner;
			_initialized = true;
			_hitCallback = callback;
		}

		public void UpdateServer() {
			MoveProjectile();
		}

		public void UpdateClient() {
			if (NetworkManager.Singleton.IsServer) 
				return;
			
			MoveProjectile();
		}

		private void MoveProjectile() {
			if (!_initialized)
				return;
			
			Vector3 oldPos = transform.position;

			transform.position += transform.forward * _speed * Time.deltaTime;

			if (Physics.Raycast(
					oldPos,
					transform.position - oldPos,
					out RaycastHit hit,
					_speed * Time.deltaTime,
					GameWorld.EntityMask)
			) {
				OnTriggerEnter(hit.collider);
			}

			_lifeTime -= Time.deltaTime;

			if (_lifeTime <= 0f) Destroy(gameObject);
		}

		private void OnTriggerEnter (Collider other) {
			if (!_initialized) 
				return;
			
			if (EntityCache.TryGetEntityComponent(other.transform.root.name, out IAttackable unit)) {
				if (unit.GetRelationship(_owner) == Relationship.Owned ||
					unit.GetRelationship(_owner) == Relationship.Friendly) 
					return;
				
				_hitCallback(true, unit);
				Destroy(gameObject);
			}
			else {
				Destroy(gameObject);
			}
		}
	}
}