using MarsTS.Buildings;
using MarsTS.Commands;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;

namespace MarsTS.Events {

	public class HarvesterDepositEvent : AbstractEvent {

		public ISelectable Harvester { get; private set; }
		public int StoredAmount { get; private set; }
		public int Capacity { get; private set; }
		public IDepositable Bank { get; private set; }

		public HarvesterDepositEvent (EventAgent _source, ISelectable _harvester, int _storedAmount, int _capacity, IDepositable _bank) : base("harvesterDeposit", _source) {
			Harvester = _harvester;
			StoredAmount = _storedAmount;
			Capacity = _capacity;
			Bank = _bank;
		}
	}
}