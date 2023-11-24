using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class PumpjackSnapping : MonoBehaviour {
		
		private List<OilDeposit> detected = new List<OilDeposit>();

		private SphereCollider detector;

		private void Awake () {
			detector = GetComponent<SphereCollider>();
		}

		public OilDeposit Snap {
			get {
				OilDeposit closestDeposit = null;
				float smallestDistance = detector.radius * 2;

				foreach (OilDeposit deposit in detected) {
					float dist = Vector3.Distance(transform.position, deposit.transform.position);

					if (dist < smallestDistance) {
						closestDeposit = deposit;
						smallestDistance = dist;
					}
				}

				return closestDeposit;
			}
		}

		protected virtual void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out OilDeposit found) && !detected.Contains(found)) {
				detected.Add(found);
			}
		}

		protected virtual void OnTriggerExit (Collider other) {
			OilDeposit toRemove = null;

			foreach (OilDeposit deposit in detected) {
				if (deposit.name == other.transform.root.name) {
					toRemove = deposit;
					break;
				}
			}

			if (toRemove != null) detected.Remove(toRemove);
		}
	}
}