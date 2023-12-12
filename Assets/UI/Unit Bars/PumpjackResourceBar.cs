using MarsTS.Buildings;
using MarsTS.Events;
using MarsTS.Units;
using MarsTS.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class PumpjackResourceBar : UnitBar {

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

		private bool DisplayBar {
			get {
				return displayBar;
			}
			set {
				if (parent.Constructed) {
					barRenderer.enabled = value;
					displayBar = value;
				}
			}
		}

		private bool displayBar;

		private Pumpjack parent;

		private void Start () {
			HasStored = false;
			barRenderer.enabled = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			parent = GetComponentInParent<Pumpjack>();

			FillLevel = (float)parent.StoredAmount / parent.OriginalAmount;

			bus.AddListener<ResourceHarvestedEvent>((_event) => {
				FillLevel = (float)_event.Deposit.StoredAmount / _event.Deposit.OriginalAmount;
			});

			bus.AddListener<UnitHoverEvent>((_event) => {
				if (_event.Status) DisplayBar = true;
				else if (!HasStored) DisplayBar = false;
			});

			bus.AddListener<UnitSelectEvent>((_event) => {
				if (_event.Status) DisplayBar = true;
				else if (!HasStored) DisplayBar = false;
			});

			bus.AddListener<ResourceHarvestedEvent>((_event) => {
				FillLevel = (float)_event.StoredAmount / _event.Capacity;

				if (FillLevel > 0f) HasStored = true;
				else HasStored = false;
			});

			bus.AddListener<HarvesterDepositEvent>((_event) => {
				FillLevel = (float)_event.StoredAmount / _event.Capacity;

				if (FillLevel > 0f) HasStored = true;
				else HasStored = false;
			});
		}
	}
}