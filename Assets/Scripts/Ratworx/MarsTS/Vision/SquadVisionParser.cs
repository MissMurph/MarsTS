using System.Collections.Generic;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Init;
using Ratworx.MarsTS.Events.Player;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;

namespace Ratworx.MarsTS.Vision
{
    public class SquadVisionParser : EntityVision
    {
        private readonly Dictionary<string, EntityVision> _squadVision = new Dictionary<string, EntityVision>();

        protected override void Awake()
        {
            base.Awake();

            bus.AddListener<SquadRegisterEvent>(OnMemberRegister);
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
                visibleTo = 0;

                foreach (EntityVision childVision in _squadVision.Values)
                {
                    visibleTo |= childVision.VisibleTo;
                }
            }
        }

        private void OnMemberDeath(UnitDeathEvent evnt)
        {
            string deadKey = evnt.Unit.GameObject.name;

            _squadVision.Remove(deadKey);
        }

        private void OnMemberInit(EntityInitEvent evnt)
        {
            if (evnt.Phase == Phase.Post) return;
            _squadVision[evnt.ParentEntity.name] = evnt.ParentEntity.GetEntityComponent<EntityVision>("vision");
        }
    }
}