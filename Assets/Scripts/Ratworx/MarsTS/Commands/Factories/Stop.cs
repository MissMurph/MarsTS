using MarsTS.Players;
using System;
using System.Collections.Generic;
using MarsTS.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public class Stop : CommandFactory<bool> {
		public override string Name => "stop";

		public override string Description => description;

		[SerializeField]
		private string description;

		public override void StartSelection () 
			=> Construct(Player.ListSelected);

		private void Construct (List<string> _selection) 
			=> ConstructCommandletServerRpc(Player.Commander.Id, _selection.ToNativeArray32(), Player.Include);

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(int _factionId, NativeArray<FixedString32Bytes> _selection, bool _inclusive) 
			=> ConstructCommandletServer(true, _factionId, _selection.ToStringList(), _inclusive);

		public override CostEntry[] GetCost () => Array.Empty<CostEntry>();

		public override void CancelSelection () {
			
		}
	}
}