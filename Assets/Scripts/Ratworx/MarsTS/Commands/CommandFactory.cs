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
		public void ConstructCommand(string commandKey, T target, Faction commander, ICollection<int> selection, bool enqueue) {
			if (NetworkManager.Singleton.IsServer)
				ConstructCommandServer(commandKey, target, commander, selection.ToArray(), enqueue);
			else
				ConstructCommandServerRpc(commandKey, target, commander.Id, selection.ToArray(), enqueue);
		}

		/// <remarks>Make sure <c>T</c> is NetworkSerializable or else you'll face runtime errors</remarks>
		[Rpc(SendTo.Server)]
		private void ConstructCommandServerRpc(string commandKey, T target, int factionId, int[] selection, bool enqueue)
			=> ConstructCommandServer(commandKey, target, TeamCache.Faction(factionId), selection, enqueue);
		
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
	}
}