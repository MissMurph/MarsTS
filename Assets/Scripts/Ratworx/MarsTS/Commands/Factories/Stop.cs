using System;
using System.Collections.Generic;
using Ratworx.MarsTS.Networking;
using Ratworx.MarsTS.Production;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Factories {

	public class Stop : CommandFactory<bool> {
		public override string Name => "stop";

		public override string Description => description;

		[SerializeField]
		private string description;

		public override void StartSelection () 
			=> Construct(Player.Player.ListSelected.ToArray());

		private void Construct (int[] _selection) 
			=> ConstructCommandletServerRpc(Player.Player.Commander.Id, _selection, Player.Player.Include);

		[Rpc(SendTo.Server)]
		private void ConstructCommandletServerRpc(int _factionId, int[] _selection, bool _inclusive) 
			=> ConstructCommandServer(true, _factionId, _selection, _inclusive);

		public override ResourceCost[] GetCost () => Array.Empty<ResourceCost>();

		public override void CancelSelection () {
			
		}
	}
}