using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Turrets {

    public class Explosion : MonoBehaviour {

        [FormerlySerializedAs("damage")] [SerializeField]
        private int _damage;

        [FormerlySerializedAs("lifeTime")] [SerializeField]
        private float _lifeTime = 0.125f;

		private Faction _owner;

		private bool _initialized = false;

		private readonly List<Collider> _hit = new List<Collider>();

		private ISelectable _attacker;

		private EventAgent _bus;

		public void Init (int damage, Faction owner, ISelectable attacker) {
			_damage = damage;
			_owner = owner;

			_attacker = attacker;
			EntityCache.TryGetEntityComponent(_attacker.GameObject.name, out _bus);
			
			_initialized = true;
		}

		private void Update () {
			if (_initialized) {
				_lifeTime -= Time.deltaTime;

				if (_lifeTime <= 0) Destroy(gameObject);
			}
		}

		private void OnTriggerEnter (Collider other) {
			_hit.Add(other);
		}

		private void OnDestroy ()
		{
			if (!NetworkManager.Singleton.IsServer) return;
			
			foreach (Collider other in _hit) {
				if (other == null) continue;
				
				if (EntityCache.TryGetEntityComponent(other.transform.root.name, out IAttackable unit)) {
					if (unit.GetRelationship(_owner) != Relationship.Owned && unit.GetRelationship(_owner) != Relationship.Friendly)
					{
						AttackUnit(unit);
					}
				}
			}
		}

		private void AttackUnit(IAttackable unit)
		{
			UnitAttackEvent attackEvent = new UnitAttackEvent(_bus, unit as ISelectable, _attacker, _damage);
				
			attackEvent.Phase = Phase.Pre;
			_bus.Global(attackEvent);

			// Captures modified damage
			int damage = attackEvent.Damage;
			unit.Attack(damage);
			
			attackEvent.Phase = Phase.Post;
			_bus.Global(attackEvent);
		}
	}
}