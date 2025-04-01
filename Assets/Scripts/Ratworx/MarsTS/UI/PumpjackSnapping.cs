using System.Collections.Generic;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.WorldObject;
using UnityEngine;

namespace Ratworx.MarsTS.UI {

    public class PumpjackSnapping : MonoBehaviour {
		
		private List<OilDeposit> _detected = new List<OilDeposit>();
		private SphereCollider _detector;

		private void Awake () {
			_detector = GetComponent<SphereCollider>();
		}

		public bool TrySnap(out Vector3 pos) {
			bool snap = false;
			pos = Vector3.zero;
			
			float smallestDistance = _detector.radius * 2;

			foreach (OilDeposit deposit in _detected) {
				float dist = Vector3.Distance(transform.position, deposit.transform.position);

				if (!(dist < smallestDistance)) 
					continue;
				
				pos = deposit.transform.position;
				smallestDistance = dist;
				snap = true;
			}

			return snap;
		}

		protected virtual void OnTriggerEnter (Collider other) {
			if (EntityCache.TryGet(other.transform.root.name, out OilDeposit found) && !_detected.Contains(found)) {
				_detected.Add(found);
			}
		}

		protected virtual void OnTriggerExit (Collider other) {
			OilDeposit toRemove = null;

			foreach (OilDeposit deposit in _detected) {
				if (deposit.name == other.transform.root.name) {
					toRemove = deposit;
					break;
				}
			}

			if (toRemove != null) _detected.Remove(toRemove);
		}
	}
}