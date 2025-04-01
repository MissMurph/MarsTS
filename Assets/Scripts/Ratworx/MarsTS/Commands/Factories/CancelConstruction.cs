using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Networking;
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

		public void Construct(List<string> selection) {
			ConstructCommandletServerRpc(Player.Player.Commander.Id, selection.ToNativeArray32(), Player.Player.Include);
		}

		[Rpc(SendTo.Server)]
		public void ConstructCommandletServerRpc(int factionId, NativeArray<FixedString32Bytes> selection, bool inclusive) {
			ConstructCommandletServer(true, factionId, selection.ToStringList(), inclusive);
		}

		public override CostEntry[] GetCost () {
			return Array.Empty<CostEntry>();
		}

		public override void CancelSelection () {
			
		}
	}
}