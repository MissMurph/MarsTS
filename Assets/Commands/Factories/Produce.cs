using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Unity.Networking.Transport.Utilities.ReliableUtility;

namespace MarsTS.Commands {

	public class Produce : CommandFactory<GameObject> {

		public override string Name { get { return "produce/" + unitPrefab.name; } }

		public override Sprite Icon { get { return unit.Icon; } }

		public override string Description { get { return description; } }

		[SerializeField]
		protected string description;

		[SerializeField]
		protected GameObject unitPrefab;

		[SerializeField]
		protected int timeRequired;

		[SerializeField]
		protected CostEntry[] cost;

		private ISelectable unit;

		private void Awake () {
			unit = unitPrefab.GetComponent<ISelectable>();
		}

		public override void StartSelection () {
			bool canAfford = true;

			foreach (CostEntry entry in cost) {
				if (Player.Commander.Resource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (canAfford) {
				foreach (KeyValuePair<string, Roster> entry in Player.Selected) {
					int lowestAmount = 999;
					ICommandable lowestOrderable = null;

					foreach (ICommandable orderable in entry.Value.Orderable) {
						if (!orderable.CanCommand(Name)) continue;
						if (orderable.Count < lowestAmount) {
							lowestAmount = orderable.Count;
							lowestOrderable = orderable;
						}
					}

					if (lowestOrderable != null) {
						//Debug.Log("ID Creating Command: " + Player.Commander.ID);
						ConstructProductionletServerRpc(Player.Commander.Id, lowestOrderable.GameObject);
					}
				}
			}
		}

		//We create separate calls for now since Productionlets are different to normal commands
		//This is due to having to serialize GameObject as a target when we don't need to
		[Rpc(SendTo.Server)]
		protected virtual void ConstructProductionletServerRpc (int _factionId, NetworkObjectReference _selection) {
			ConstructProductionletServer(_factionId, _selection);
		}

		protected virtual void ConstructProductionletServer (int _factionId, NetworkObjectReference _selection) {
			bool canAfford = true;

			Faction player = TeamCache.Faction(_factionId);

			foreach (CostEntry entry in cost) {
				if (player.Resource(entry.key).Amount < entry.amount) {
					canAfford = false;
					break;
				}
			}

			if (!canAfford) return;

			ProduceCommandlet order = Instantiate(orderPrefab) as ProduceCommandlet;

			order.Init("produce", Name, unitPrefab, TeamCache.Faction(_factionId), timeRequired, cost);

			if (EntityCache.TryGet(((GameObject)_selection).name, out ICommandable unit)) {
				unit.Order(order, true);
			}

			foreach (CostEntry entry in cost) {
				player.Resource(entry.key).Withdraw(entry.amount);
			}
		}

		public override CostEntry[] GetCost () {
			List<CostEntry> spool = new List<CostEntry>();

			foreach (CostEntry entry in cost) {
				spool.Add(entry);
			}

			CostEntry time = new CostEntry();
			time.key = "time";
			time.amount = timeRequired;

			spool.Add(time);

			return spool.ToArray();
		}

		public override void CancelSelection () {
			
		}
	}

	[Serializable]
	public class CostEntry {
		public string key;
		public int amount;
	}
}