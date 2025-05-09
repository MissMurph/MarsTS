using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class WorkBar : UnitBar
    {
        private void Start() {
            _barRenderer.enabled = false;

            EventAgent bus = GetComponentInParent<EventAgent>();

            bus.AddListener<CommandWorkEvent>(OnWorkStep);
        }

        private void OnWorkStep(CommandWorkEvent evnt) {
            if (evnt.Progress >= 1f) {
                _barRenderer.enabled = false;
            }
            else {
                UpdateBarWithFillLevel(evnt.Progress);
                _barRenderer.enabled = true;
            }
        }
    }
}