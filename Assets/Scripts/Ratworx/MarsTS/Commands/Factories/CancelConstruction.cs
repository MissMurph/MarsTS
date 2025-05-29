using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Production;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories {

	public class CancelConstruction : CommandFactory<bool> {

		public override string Name => "cancelConstruction";

		public override string Description => description;

		[SerializeField]
		private string description;

		public override void StartSelection () {
			Construct(Player.Player.ListSelected);
		}

		public void Construct(List<int> selection) {
			ConstructCommandletServerRpc(Player.Player.Commander.Id, selection.ToArray(), Player.Player.Include);
		}

		[Rpc(SendTo.Server)]
		public void ConstructCommandletServerRpc(int factionId, int[] selection, bool inclusive) {
			ConstructCommandletServer(true, factionId, selection, inclusive);
		}

		public override ResourceCost[] GetCost () {
			return Array.Empty<ResourceCost>();
		}

		public override void CancelSelection () {
			
		}
	}
}