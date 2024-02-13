using MarsTS.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.UI {

    public class WorkBar : UnitBar {

		private void Start () {
			barRenderer.enabled = false;

			EventAgent bus = GetComponentInParent<EventAgent>();

			bus.AddListener<WorkEvent>(OnWorkStep);
			bus.AddListener<CommandWorkEvent>(OnWorkStep);
		}

		private void OnWorkStep (WorkEvent _event) {
			if (_event.CurrentWork < _event.WorkRequired) {
				barRenderer.enabled = true;
				FillLevel = _event.CurrentWork / _event.WorkRequired;
			}
			else {
				barRenderer.enabled = false;
			}
		}

		private void OnWorkStep (CommandWorkEvent _event) {
			if (_event.Work.CurrentWork < _event.Work.WorkRequired) {
				barRenderer.enabled = true;
				FillLevel = _event.Work.CurrentWork / _event.Work.WorkRequired;
			}
			else {
				barRenderer.enabled = false;
			}
		}
	}
}