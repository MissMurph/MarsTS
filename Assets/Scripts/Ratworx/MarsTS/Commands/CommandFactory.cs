using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands.Factories;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Registry;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands {

	public abstract class CommandFactory<T> : CommandFactory
	{
		//Only call this on the server
		protected virtual void ConstructCommandletServer (T target, int factionId, int[] selection, bool inclusive) {
			Commandlet<T> order = Instantiate(orderPrefab);

			order.Init(Name, target, TeamCache.Faction(factionId));

			foreach (int entityId in selection) {
				if (EntityCache.TryGetEntity(entityId, out Entity entity)
				&& entity.TryGetEntityComponent(out ICommandable unit))
					unit.Order(order, inclusive);
				else
					RatLogger.Warning?.Log($"ICommandable on Unit {entityId} not found! Command {Name} being ignored by unit!");
			}
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
		public abstract ResourceCost[] GetCost ();

		private void Start() {
			//GetComponent<NetworkObject>().Spawn();
		}

		public string RegistryType => "command_factory";
		public string RegistryKey => Name;
		public CommandFactory GetEntityComponent() => this;
	}
}