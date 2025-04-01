using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MarsTS.Entities;
using MarsTS.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

    public class Sneak : CommandFactory<bool> {
		public override string Name { get { return "sneak"; } }

		public override Type TargetType { get { return typeof(bool); } }

		public override string Description { get { return description; } }

		[SerializeField]
		private string description;

		[SerializeField]
		private float deactivateCooldown;

		[SerializeField]
		private float reactivateCooldown;

		public override void StartSelection () {
			int totalWithSneak = 0;
			int totalSneakActive = 0;

			//Inspect all selected to make all units using this ability match up with others that are active using
			foreach (Roster rollup in Player.Selected.Values) {
				if (rollup.Commands.Contains(Name)) {
					totalWithSneak += rollup.Count;

					foreach (ICommandable unit in rollup.Orderable) {
						if (unit.Active.Count == 0) continue;

						foreach (string activeCommand in unit.Active) {
							if (activeCommand == Name) totalSneakActive++;
						}
					}
				}
			}

			Construct(totalWithSneak > totalSneakActive);
		}

		public void Construct(bool status) {
			ConstructCommandletServerRpc(
				status,
				Player.Commander.Id,
				Player.ListSelected.ToNativeArray32(),
				Player.Include
			);
		}

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(
			bool status, 
			int factionId, 
			NativeArray<FixedString32Bytes> selection, 
			bool inclusive
		) {
			ConstructCommandletServer(status, factionId, selection.ToStringList(), inclusive);
		}

		protected override void ConstructCommandletServer(bool target, int factionId, ICollection<string> selection, bool inclusive) {
			SneakCommandlet order = (SneakCommandlet)Instantiate(orderPrefab);

			order.InitSneak(Name, target, TeamCache.Faction(factionId), deactivateCooldown, reactivateCooldown);

			foreach (string entity in selection) {
				if (EntityCache.TryGet(entity, out ICommandable unit))
					unit.Order(order, inclusive);
				else
					Debug.LogWarning($"ICommandable on Unit {entity} not found! Command {Name} being ignored by unit!");
			}
		}


		public override CostEntry[] GetCost () {
			return new CostEntry[1] { new CostEntry { key = "time", amount = 60} };
		}

		public override void CancelSelection () {
			
		}
	}
}