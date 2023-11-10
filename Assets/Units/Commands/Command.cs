using MarsTS.Players;
using MarsTS.Teams;
using MarsTS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MarsTS.Units.Commands {

	public abstract class Command<T> : Command {

		public Commandlet Construct (T _target) {
			return new Commandlet<T>(Name, _target, Player.Main);
		}
		
		public override Type TargetType { get { return typeof(T); } }
	}

	public abstract class Command : MonoBehaviour {

		public abstract string Name { get; }
		public abstract Type TargetType { get; }
		public abstract void StartSelection ();

		public Color IconColor;
		public CursorSprite Pointer;
	}

	public class Commandlet<T> : Commandlet {
		
		public T Target;
		public override Type TargetType { get { return typeof(T); } }

		public Commandlet (string name, T target, Faction commander) {
			Target = target;
			Name = name;
			Commander = commander;
		}
	}

	public abstract class Commandlet {

		public abstract Type TargetType { get; }
		public string Name { get; protected set; }
		public Faction Commander { get; protected set; }

		public Commandlet<T> Get<T> () {
			if (typeof(T).Equals(TargetType)) return this as Commandlet<T>;
			else throw new ArgumentException("Commandlet target type " + TargetType + " does not match given type " + typeof(T) + ", cannot return Commandlet!");
		}
	}
}