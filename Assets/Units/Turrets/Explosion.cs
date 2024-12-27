using MarsTS.Entities;
using MarsTS.Teams;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Events;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.UI.GridLayoutGroup;

namespace MarsTS.Units {

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
			EntityCache.TryGet(_attacker.GameObject.name, out _bus);
			
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
			if (!NetworkManager.Singleton.IsServer) ;
			
			foreach (Collider other in _hit) {
				if (other == null) continue;
				
				if (EntityCache.TryGet(other.transform.root.name, out IAttackable unit)) {
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