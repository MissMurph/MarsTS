using MarsTS.Buildings;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.Units;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using static Unity.Networking.Transport.Utilities.ReliableUtility;

namespace MarsTS.Commands {

	public class Produce : CommandFactory<GameObject> {

		public override string Name => "produce/" + unitPrefab.name;

		public override Sprite Icon => _unit.Icon;

		public override string Description => description;

		[SerializeField]
		protected string description;

		[SerializeField]
		protected GameObject unitPrefab;

		[SerializeField]
		protected int timeRequired;

		[SerializeField]
		protected CostEntry[] Cost;

		private ISelectable _unit;

		private void Awake () {
			_unit = unitPrefab.GetComponent<ISelectable>();
		}

		public override void StartSelection () {
			if (!CanFactionAfford(Player.Commander)) return;

			foreach (KeyValuePair<string, Roster> entry in Player.Selected) {
				int lowestAmount = 9999;
				ICommandable lowestCommandable = null;

				foreach (ICommandable commandable in entry.Value.Orderable) {
					if (!commandable.CanCommand(Name)
					    || commandable.Count >= lowestAmount) 
						continue;
					
					lowestAmount = commandable.Count;
					lowestCommandable = commandable;
				}

				if (lowestCommandable != null) {
					ConstructProductionletServerRpc(Player.Commander.Id, lowestCommandable.GameObject.name);
				}
			}
		}

		//We create separate calls for now since Productionlets are different to normal commands
		//This is due to having to serialize GameObject as a target when we don't need to
		[Rpc(SendTo.Server)]
		protected virtual void ConstructProductionletServerRpc (int factionId, string selection) {
			ConstructProductionletServer(factionId, selection);
		}

		protected virtual void ConstructProductionletServer (int factionId, string selection) 
		{
			Faction faction = TeamCache.Faction(factionId);
			
			if (!CanFactionAfford(faction)) 
				return;

			ProduceCommandlet order = Instantiate(orderPrefab) as ProduceCommandlet;

			order.Init("produce", Name, unitPrefab, TeamCache.Faction(factionId), timeRequired, Cost);

			if (EntityCache.TryGet(selection, out ICommandable unit)) {
				unit.Order(order, true);
			}

			WithdrawResourcesFromFaction(faction);
		}

		public override CostEntry[] GetCost () {
			List<CostEntry> spool = Cost.ToList();

			CostEntry time = new CostEntry {
				key = "time",
				amount = timeRequired
			};

			spool.Add(time);

			return spool.ToArray();
		}

		public override void CancelSelection () {
			
		}
		
		protected bool CanFactionAfford(Faction faction)
			=> !Cost.Any(entry => faction.GetResource(entry.key).Amount < entry.amount);
		
		protected void WithdrawResourcesFromFaction(Faction faction) {
			foreach (CostEntry entry in Cost) 
				faction.GetResource(entry.key).Withdraw(entry.amount);
		}
	}

	[Serializable]
	public class CostEntry {
		public string key;
		public int amount;
	}
}