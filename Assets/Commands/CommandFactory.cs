using MarsTS.Entities;
using MarsTS.Teams;
using MarsTS.UI;
using System;
using System.Collections.Generic;
using MarsTS.Prefabs;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public abstract class CommandFactory<T> : CommandFactory
	{
		//Only call this on the server
		protected virtual void ConstructCommandletServer (T target, int factionId, ICollection<string> selection, bool inclusive) {
			Commandlet<T> order = Instantiate(orderPrefab);

			order.Init(Name, target, TeamCache.Faction(factionId));

			foreach (string entity in selection) {
				if (EntityCache.TryGet(entity, out ICommandable unit))
					unit.Order(order, inclusive);
				else
					Debug.LogWarning($"ICommandable on Unit {entity} not found! Command {Name} being ignored by unit!");
			}
		}

		public virtual void Construct(T target, int factionId, List<string> selection, bool inclusive) {
			throw new NotImplementedException();
		}

		public Commandlet<T> Prefab => orderPrefab;
		
		[SerializeField]
		protected Commandlet<T> orderPrefab;
		
		public override Type TargetType => typeof(T);
	}

	public abstract class CommandFactory : NetworkBehaviour, IRegistryObject<CommandFactory>
	{
		public abstract string Name { get; }
		public abstract Type TargetType { get; }
		public virtual Sprite Icon => icon;
		public abstract string Description { get; }

		[SerializeField]
		protected Sprite icon;

		public CursorSprite Pointer;

		public abstract void StartSelection ();
		public abstract void CancelSelection ();
		public abstract CostEntry[] GetCost ();

		private void Start() {
			//GetComponent<NetworkObject>().Spawn();
		}

		public string RegistryType => "command_factory";
		public string RegistryKey => Name;
		public CommandFactory Get() => this;
	}
}