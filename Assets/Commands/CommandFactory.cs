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

		public abstract void Construct (T _target);

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