using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using Ratworx.MarsTS.Registry;
using Ratworx.MarsTS.Teams;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Commands
{
	public abstract class CommandFactory<T> : CommandFactory
	{
		/// <remarks>Make sure <c>T</c> is NetworkSerializable or else you'll face runtime errors</remarks>
		public virtual void ConstructCommand(string commandKey, T target, Faction commander, ICollection<int> selection, bool enqueue) {
			if (NetworkManager.Singleton.IsServer)
				ConstructCommandServer(commandKey, target, commander, selection.ToArray(), enqueue);
			else
				ConstructCommandServerRpc(commandKey, target, commander.Id, selection.ToArray(), enqueue);
		}

		/// <remarks>This needs explicit declarations of T so Netcode can generate the RPC code.</remarks>
		[Rpc(SendTo.Server)]
		protected virtual void ConstructCommandServerRpc(string commandKey, T target, int factionId, int[] selection,
			bool enqueue)
			=> throw new NotImplementedException(
				$"You must create an explicit implementation of this method on your factory!");
		
		//Only call this on the server
		protected void ConstructCommandServer(string commandKey, T target, Faction commander, IEnumerable<int> selection, bool enqueue) {
			Commandlet<T> order = Instantiate(OrderPrefab);

			order.Init(commandKey, target, commander);

			foreach (int entityId in selection) {
				if (EntityCache.TryGetEntity(entityId, out Entity entity)
				&& entity.TryGetEntityComponent(out ICommandable unit))
					unit.Order(order, enqueue);
				else
					RatLogger.Warning?.Log($"ICommandable on Unit {entityId} not found! Command {commandKey} being ignored by unit!");
			}
		}

		public override void TryConstructCommandFromEntity(
			string commandKey,
			Entity targetEntity,
			Faction commander,
			ICollection<int> selection,
			bool enqueue
		) {
			if (!targetEntity.TryGetEntityComponent(out T target)) {
				RatLogger.Warning?.Log($"Couldn't find target type {nameof(T)} on target entity, cancelling command.");
				return;
			}
			
			ConstructCommand(commandKey, target, commander, selection, enqueue);
		}

		[FormerlySerializedAs("orderPrefab")]
		[SerializeField]
		protected Commandlet<T> OrderPrefab;
	}

	public abstract class CommandFactory : NetworkBehaviour, 
										   IRegistryObject<CommandFactory>
	{
		public abstract string Name { get; }
		public string RegistryType => "command_factory";
		public string RegistryKey => Name;
		public CommandFactory GetEntityComponent() => this;
		/// <summary>
		/// Use this when you don't have the explicit Type on hand (using command evaluation)
		/// </summary>
		public abstract void TryConstructCommandFromEntity(
			string commandKey,
			Entity targetEntity,
			Faction commander,
			ICollection<int> selection,
			bool enqueue
		);
	}
}