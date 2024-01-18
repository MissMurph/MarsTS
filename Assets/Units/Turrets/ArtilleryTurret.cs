using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Units {

    public class ArtilleryTurret : ProjectileTurret {

		private Quaternion startingPos;

		private bool isDeployed;

		private GameObject rangeIndicator;

		protected override void Awake () {
			base.Awake();

			rangeIndicator = transform.root.Find("RangeIndicator").gameObject;
		}

		private void Start () {
			startingPos = barrel.transform.localRotation;

			bus.AddListener<DeployEvent>(OnDeploy);
			bus.AddListener<UnitSelectEvent>(OnSelect);
			bus.AddListener<UnitHoverEvent>(OnHover);
		}

		protected override void Update () {
			if (isDeployed) base.Update();
			else barrel.transform.localRotation = startingPos;
		}

		private void OnDeploy (DeployEvent _event) {
			isDeployed = _event.IsDeployed;
			rangeIndicator.SetActive(_event.IsDeployed);
		}

		private void OnSelect (UnitSelectEvent _event) {
			if (isDeployed && _event.Status) {
				rangeIndicator.SetActive(true);
			}
			else {
				rangeIndicator.SetActive(false);
			}
		}

		private void OnHover (UnitHoverEvent _event) {
			if (isDeployed && _event.Status) {
				rangeIndicator.SetActive(true);
			}
			else {
				rangeIndicator.SetActive(false);
			}
		}
	}
}