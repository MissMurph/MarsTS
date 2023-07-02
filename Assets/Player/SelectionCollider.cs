using MarsTS.Events;
using MarsTS.Units;
using MarsTS.Units.Cache;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Players {

    public class SelectionCollider : MonoBehaviour {

		public List<Unit> hitTransforms = new List<Unit>();

		private void OnTriggerEnter (Collider other) {
			hitTransforms.Add(UnitCache.Get(other.transform.parent.name));
		}

		private void OnDestroy () {
			Player.Main.SelectUnit(hitTransforms.ToArray());
			EventBus.Global(new SelectEvent(true, Player.Selected));
		}
	}
}