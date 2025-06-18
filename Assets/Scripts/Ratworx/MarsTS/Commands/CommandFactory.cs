using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Production;
using Ratworx.MarsTS.Registry;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands
{
	public abstract class CommandFactory<T> : CommandFactory
	{
		/// <remarks>Make sure <c>T</c> is NetworkSerializable or else you'll face runtime errors</remarks>
		public void ConstructCommand(T target, Faction commander, ICollection<int> selection, bool enqueue) {
			if (NetworkManager.Singleton.IsServer)
				ConstructCommandServer(target, commander, selection.ToArray(), enqueue);
			else
				ConstructCommandServerRpc(target, commander.Id, selection.ToArray(), enqueue);
		}

		/// <remarks>Make sure <c>T</c> is NetworkSerializable or else you'll face runtime errors</remarks>
		[Rpc(SendTo.Server)]
		private void ConstructCommandServerRpc(T target, int factionId, int[] selection, bool enqueue)
			=> ConstructCommandServer(target, TeamCache.Faction(factionId), selection, enqueue);
		
		//Only call this on the server
		protected void ConstructCommandServer(T target, Faction commander, IEnumerable<int> selection, bool enqueue) {
			Commandlet<T> order = Instantiate(orderPrefab);

			order.Init(Name, target, commander);

			foreach (int entityId in selection) {
				if (EntityCache.TryGetEntity(entityId, out Entity entity)
				&& entity.TryGetEntityComponent(out ICommandable unit))
					unit.Order(order, enqueue);
				else
					RatLogger.Warning?.Log($"ICommandable on Unit {entityId} not found! Command {Name} being ignored by unit!");
			}
		}

		public Commandlet<T> Prefab => orderPrefab;
		
		[SerializeField]
		protected Commandlet<T> orderPrefab;
		
		public override Type TargetType => typeof(T);
	}

	public abstract class CommandFactory : NetworkBehaviour, 
										   IRegistryObject<CommandFactory>
	{
		public abstract string Name { get; }
		public abstract Type TargetType { get; }
		public virtual Sprite Icon => icon;
		public abstract string Description { get; }

		[SerializeField]
		protected Sprite icon;

		[FormerlySerializedAs("Pointer")] [SerializeField] public CursorSprite pointer;

		public abstract void StartSelection ();
		public abstract void CancelSelection ();
		public abstract ResourceCost[] GetCost ();

		public string RegistryType => "command_factory";
		public string RegistryKey => Name;
		public CommandFactory GetEntityComponent() => this;
	}
}