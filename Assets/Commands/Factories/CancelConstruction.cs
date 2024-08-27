using System;
using System.Collections.Generic;
using MarsTS.Networking;
using MarsTS.Players;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public class CancelConstruction : CommandFactory<bool> {

		public override string Name => "cancelConstruction";

		public override string Description => description;

		[SerializeField]
		private string description;

		public override void StartSelection () {
			Construct(Player.ListSelected);
		}

		public void Construct(List<string> selection) {
			ConstructCommandletServerRpc(Player.Commander.Id, selection.ToNativeArray32(), Player.Include);
		}

		[Rpc(SendTo.Server)]
		public void ConstructCommandletServerRpc(int factionId, NativeArray<FixedString32Bytes> selection, bool inclusive) {
			ConstructCommandletServer(true, factionId, selection.ToList(), inclusive);
		}

		public override CostEntry[] GetCost () {
			return Array.Empty<CostEntry>();
		}

		public override void CancelSelection () {
			
		}
	}
}