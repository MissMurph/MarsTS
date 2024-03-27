using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Commands {

	public abstract class CommandFactory<T> : CommandFactory {

		public virtual void Construct (T _target, NetworkObjectReference[] _selection) {
			ConstructCommandletServerRpc(_target, Player.Commander.ID, _selection, Player.Include);
		}

		[Rpc(SendTo.Server)]
		protected virtual void ConstructCommandletServerRpc (T _target, int _factionId, NetworkObjectReference[] _selection, bool _inclusive) {
			ConstructCommandletServer(_target, _factionId, _selection, _inclusive);
		}

		protected virtual void ConstructCommandletServer (T _target, int _factionId, NetworkObjectReference[] _selection, bool _inclusive) {
			Commandlet<T> order = Instantiate(orderPrefab);

			order.Init(Name, _target, TeamCache.Faction(_factionId));

			foreach (NetworkObjectReference objectRef in _selection) {
				if (EntityCache.TryGet(((GameObject)objectRef).name, out ICommandable unit)) {
					unit.Order(order, _inclusive);
				}
			}
		}

		[SerializeField]
		protected Commandlet<T> orderPrefab;
		
		public override Type TargetType { get { return typeof(T); } }
	}

	public abstract class CommandFactory : NetworkBehaviour {

		public abstract string Name { get; }
		public abstract Type TargetType { get; }
		public virtual Sprite Icon { get { return icon; } }
		public abstract string Description { get; }

		[SerializeField]
		protected Sprite icon;

		public Color IconColor;
		public CursorSprite Pointer;

		public abstract void StartSelection ();
		public abstract void CancelSelection ();
		public abstract CostEntry[] GetCost ();
	}

	
}