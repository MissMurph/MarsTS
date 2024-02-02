using MarsTS.Commands;
using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class CooldownEvent : AbstractEvent {

		public string CommandKey { get; private set; }
		public ISelectable Unit { get; private set; }
		public Cooldown Cooldown { get; private set; }
		public bool Complete { get { return Cooldown.timeRemaining <= 0f; } }

		public CooldownEvent (EventAgent _source, string _command, ISelectable _unit, Cooldown _cooldown) : base("cooldown", _source) {
			Cooldown = _cooldown;
			CommandKey = _command;
			Unit = _unit;
		}
	}
}