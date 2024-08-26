using MarsTS.Entities;
using MarsTS.Teams;
using MarsTS.UI;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Commands {

	public abstract class CommandFactory<T> : CommandFactory
	{
		//Only call this on the server
		protected virtual void ConstructCommandletServer (T _target, int _factionId, ICollection<string> _selection, bool _inclusive) {
			Commandlet<T> order = Instantiate(orderPrefab);

			order.Init(Name, _target, TeamCache.Faction(_factionId));

			foreach (string entity in _selection) {
				if (EntityCache.TryGet(entity, out ICommandable unit))
					unit.Order(order, _inclusive);
				else
					Debug.LogWarning($"ICommandable on Unit {entity} not found! Command {Name} being ignored by unit!");
			}
		}

		[SerializeField]
		protected Commandlet<T> orderPrefab;
		
		public override Type TargetType => typeof(T);
	}

	public abstract class CommandFactory : NetworkBehaviour {

		public abstract string Name { get; }
		public abstract Type TargetType { get; }
		public virtual Sprite Icon => icon;
		public abstract string Description { get; }

		[SerializeField]
		protected Sprite icon;

		public CursorSprite Pointer;

		public abstract void StartSelection ();
		public abstract void CancelSelection ();
		public abstract CostEntry[] GetCost ();
	}
}