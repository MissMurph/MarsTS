using System;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Units;
using UnityEngine;
using Unity.Netcode;

namespace Ratworx.MarsTS.Entities 
{
	// TODO: Make this USE an attribute and be a separate component, rather than inherit attribute
    public class HealthAttribute : EntityAttribute, 
								   IAttackable,
								   IEntityComponent<IAttackable>
	{
		public int Health => Value;
		public int MaxHealth => _maxHealth;
		public Entity Entity { get; private set; }

		private EventAgent _eventAgent;
		private UnitOwnership _ownership;

		private void Awake() {
			_key = "health";
			Entity = GetComponent<Entity>();
			_eventAgent = GetComponent<EventAgent>();
			_ownership = GetComponent<UnitOwnership>();
		}

		private void Start() {
			_eventAgent.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
		}

		public override void OnNetworkSpawn() {
			base.OnNetworkSpawn();
			
			OnAttributeChange += OnHurt;
		}

		public void Attack(int damage) {
			if (Health <= 0) return;
			if (damage < 0 && Health >= MaxHealth) return;

			UnitHurtEvent hurtEvent = new UnitHurtEvent(this, damage);
			hurtEvent.Phase = Phase.Pre;
			_eventAgent.PostGlobal(hurtEvent);

			damage = hurtEvent.Damage;
			Value -= damage;

			hurtEvent.Phase = Phase.Post;
			_eventAgent.PostGlobal(hurtEvent);
		}

		public Relationship GetRelationship(Faction player) => _ownership.GetRelationship(player);
		
		[SerializeField]
		private int _maxHealth;

		public GameObject GameObject => gameObject;
		
		protected void OnHurt(int oldHealth, int newHealth)
		{
			if (Health <= 0)
			{
				_eventAgent.PostGlobal(new UnitDeathEvent(Entity));

				if (NetworkManager.Singleton.IsServer)
					Destroy(gameObject, 0.1f);
			}
			else
			{
				UnitHurtEvent hurtEvent = new UnitHurtEvent(this, oldHealth - newHealth);
				hurtEvent.Phase = Phase.Post;
				_eventAgent.PostGlobal(hurtEvent);
			}
		}
		
		protected virtual void OnUnitInfoDisplayed(UnitInfoEvent _event) {
			HealthInfo info = _event.Info.Module<HealthInfo>("health");
			info.CurrentUnit = this;
		}

		public void SetMaxHealth(int newValue) {
			_maxHealth = newValue;
		}

		public IAttackable Get() => this;
	}
}