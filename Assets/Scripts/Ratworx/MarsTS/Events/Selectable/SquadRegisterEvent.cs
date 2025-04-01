using MarsTS.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Events {

	public class SquadRegisterEvent : AbstractEvent {

		public ISelectable Host { get; private set; }

		public ISelectable RegisteredMember { get; private set; }

		public SquadRegisterEvent (EventAgent _source, ISelectable _host, ISelectable _member) : base("squadRegister", _source) {
			Host = _host;
			RegisteredMember = _member;
		}
	}
}