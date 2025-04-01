using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class WorkBar : UnitBar {

		private void Start () {
			_barRenderer.enabled = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			bus.AddListener<WorkEvent>(OnWorkStep);
			bus.AddListener<CommandWorkEvent>(OnWorkStep);
		}

		private void OnWorkStep (WorkEvent _event) {
			if (_event.CurrentWork < _event.WorkRequired) {
				_barRenderer.enabled = true;
				UpdateBarWithFillLevel(_event.CurrentWork / _event.WorkRequired);
			}
			else {
				_barRenderer.enabled = false;
			}
		}

		private void OnWorkStep (CommandWorkEvent _event) {
			if (_event.Work.CurrentWork < _event.Work.WorkRequired) {
				_barRenderer.enabled = true;
				UpdateBarWithFillLevel(_event.Work.CurrentWork / _event.Work.WorkRequired);
			}
			else {
				_barRenderer.enabled = false;
			}
		}
	}
}