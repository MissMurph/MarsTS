using System;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.Units;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Entities 
{
    public class HealthAttribute : EntityAttribute, IAttackable
	{
		public int Health => Value;
		public int MaxHealth => _maxHealth;
		public Entity Entity { get; private set; }

		private EventAgent _eventAgent;

		private void Awake() {
			_key = "health";
			Entity = GetComponent<Entity>();
			_eventAgent = GetComponent<EventAgent>();
		}

		public override void OnNetworkSpawn() {
			base.OnNetworkSpawn();
			
			OnAttributeChange += OnHurt;
		}

		public void Attack(int damage) {
			if (Health <= 0) return;
			if (damage < 0 && Health >= MaxHealth) return;

			UnitHurtEvent hurtEvent = new UnitHurtEvent(_eventAgent, this, damage);
			hurtEvent.Phase = Phase.Pre;
			_eventAgent.PostGlobal(hurtEvent);

			damage = hurtEvent.Damage;
			Value -= damage;

			hurtEvent.Phase = Phase.Post;
			_eventAgent.PostGlobal(hurtEvent);
		}

		public Relationship GetRelationship(Faction player) => throw new NotImplementedException();
		
		[SerializeField]
		private int _maxHealth;

		public GameObject GameObject => gameObject;
		public IUnitInterface UnitInterface { get; }
		
		protected void OnHurt(int oldHealth, int newHealth)
		{
			if (Health <= 0)
			{
				// _eventAgent.Global(new UnitDeathEvent(_eventAgent, this));

				if (NetworkManager.Singleton.IsServer)
					Destroy(gameObject, 0.1f);
			}
			else
			{
				UnitHurtEvent hurtEvent = new UnitHurtEvent(_eventAgent, this, oldHealth - newHealth);
				hurtEvent.Phase = Phase.Post;
				_eventAgent.PostGlobal(hurtEvent);
			}
		}
    }
}