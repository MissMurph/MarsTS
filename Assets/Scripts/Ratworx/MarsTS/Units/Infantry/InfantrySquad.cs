using System;
using System.Collections.Generic;
using System.Linq;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Ratworx.MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ratworx.MarsTS.Units.Infantry
{
    public class InfantrySquad : NetworkBehaviour
    {
        


        /*protected Entity _entityComponent;

        

        protected EventAgent _bus;


        protected SquadVisionParser _squadVisibility;


        private bool _isInitialized = false;
        private bool _isInitializing = false;

        

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsServer) AttachServerListeners();
            if (NetworkManager.Singleton.IsClient) AttachClientListeners();
        }

        protected virtual void AttachServerListeners()
        {
            _bus.AddListener<CommandStartEvent>(ExecuteOrder);
        }

        protected virtual void AttachClientListeners()
        {
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
        }

        protected virtual void Update() {
            if (!_isInitialized) {
                if (!_isInitializing) {
                    _entityComponent.OnEntityInit += OnSquadEntityInit;
                    _isInitializing = true;
                }
                
                return;
            }
            
            
        }

        private void ForwardHurtEvent(UnitHurtEvent _event)
        {
            UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, _event.Damage);
            hurtEvent.Phase = Phase.Post;
            _bus.PostGlobal(hurtEvent);
        }

        

        protected virtual void ExecuteOrder(CommandStartEvent _event)
        {
            foreach (MemberEntry entry in _members.Values)
            {
                entry.Member.Order(_event.Command, false);
            }
        }

        protected virtual void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                HealthInfo info = _event.Info.Module<HealthInfo>("health");
                info.CurrentUnit = this;
            }
        }*/
    }
}