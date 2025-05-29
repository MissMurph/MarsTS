using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;

namespace Ratworx.MarsTS.Vision
{
    public class SquadVisionParser : UnitVision
    {
        private readonly Dictionary<string, UnitVision> _squadVision = new Dictionary<string, UnitVision>();

        protected override void Awake()
        {
            base.Awake();

            Bus.AddListener<SquadRegisterEvent>(OnMemberRegister);
        }

        public void OnMemberRegister(SquadRegisterEvent evnt)
        {
            EventAgent unitEvents = evnt.RegisteredMember.GameObject.GetComponent<EventAgent>();
            unitEvents.AddListener<UnitDeathEvent>(OnMemberDeath);
            unitEvents.AddListener<EntityInitEvent>(OnMemberInit);
        }

        protected override void OnVisionUpdate(VisionUpdateEvent evnt)
        {
            if (evnt.Phase == Phase.Post)
            {
                VisibilityMask = 0;

                foreach (UnitVision childVision in _squadVision.Values)
                {
                    VisibilityMask |= childVision.VisibleTo;
                }
            }
        }

        private void OnMemberDeath(UnitDeathEvent evnt)
        {
            string deadKey = evnt.Entity.gameObject.name;

            _squadVision.Remove(deadKey);
        }

        private void OnMemberInit(EntityInitEvent evnt)
        {
            if (evnt.Phase == Phase.Post) return;
            _squadVision[evnt.Entity.name] = evnt.Entity.GetEntityComponent<UnitVision>("vision");
        }
    }
}