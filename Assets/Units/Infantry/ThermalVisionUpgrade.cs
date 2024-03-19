using MarsTS.Events;
using MarsTS.Vision;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class ThermalVisionUpgrade : MonoBehaviour {

		private ISelectable parent;
		private StealthSensor vision;

		[SerializeField]
		private string researchKey = "thermalVision";

		private void Awake () {
			vision = GetComponent<StealthSensor>();
			parent = GetComponentInParent<ISelectable>();
		}

		private void Start () {
			if (parent.Owner.IsResearched(researchKey)) {
				vision.Detecting = true;
			}
			else {
				vision.Detecting = false;
				EventBus.AddListener<ResearchCompleteEvent>(OnResearch);
			}
		}

		private void OnResearch (ResearchCompleteEvent _event) {
			if (_event.Producer.Owner == parent.Owner && _event.Tech.key == researchKey) {
				EventBus.RemoveListener<ResearchCompleteEvent>(OnResearch);

				vision.Detecting = true;
			}
		}
	}
}