using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;

namespace Ratworx.MarsTS.UI.Unit_Bars
{
    public class WorkBar : UnitBar
    {
        private void Start() {
            BarRenderer.enabled = false;

            EventAgent bus = GetComponentInParent<EventAgent>();

            bus.AddListener<CommandWorkEvent>(OnWorkStep);
        }

        private void OnWorkStep(CommandWorkEvent evnt) {
            if (evnt.Progress >= 1f) {
                BarRenderer.enabled = false;
            }
            else {
                UpdateBarWithFillLevel(evnt.Progress);
                BarRenderer.enabled = true;
            }
        }
    }
}