using MarsTS.Events;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace MarsTS.UI {

    public class ResourceBar : UnitBar {

		private bool HasStored {
			get {
				return hasStored;
			}
			set {
				barRenderer.enabled = value;
				hasStored = value;
			}
		}

        private bool hasStored;

		private void Start () {
			HasStored = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			ISelectable parent = GetComponentInParent<ISelectable>();

			if (parent is IHarvestable deposit) {
				FillLevel = (float)deposit.StoredAmount / deposit.OriginalAmount;

				bus.AddListener<ResourceHarvestedEvent>((_event) => {
					FillLevel = (float)_event.Deposit.StoredAmount / _event.Deposit.OriginalAmount;
				});

				bus.AddListener<UnitHoverEvent>((_event) => {
					if (_event.Status) barRenderer.enabled = true;
					else if (!HasStored) barRenderer.enabled = false;
				});

				bus.AddListener<UnitSelectEvent>((_event) => {
					if (_event.Status) barRenderer.enabled = true;
					else if (!HasStored) barRenderer.enabled = false;
				});
			}
			else {
				ResourceStorage localStorage = GetComponentInParent<ResourceStorage>();

				FillLevel = (float)localStorage.Amount / localStorage.Capacity;

				bus.AddListener<HarvesterExtractionEvent>((_event) => {
					FillLevel = (float)_event.StoredAmount / _event.Capacity;

					if (FillLevel > 0f) HasStored = true;
					else HasStored = false;
				});
			}
		}
	}
}