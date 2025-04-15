using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.UI.Unit_Pane;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public abstract class AbstractUnit : NetworkBehaviour,
        IEntityComponent<AbstractUnit>
    {
        public GameObject GameObject => gameObject;

        /*	ITaggable Properties	*/

        public string Key => "selectable";
        
        /*	Unit Fields	*/

        private Entity _entity;

        protected Rigidbody Body;
        
        protected EventAgent Bus;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody>();
            _entity = GetComponent<Entity>();
            Bus = GetComponent<EventAgent>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsClient) AttachClientListeners();
        }

        protected void AttachClientListeners()
        {
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
            // Bus.AddListener<CommandStartEvent>(ExecuteOrder);
        }
        

        

        

        public AbstractUnit Get() => this;

        

        protected virtual void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            /*if (ReferenceEquals(_event.Unit, this))
            {
                HealthInfo info = _event.Info.Module<HealthInfo>("health");
                // info.CurrentUnit = this;
            }*/
        }

        /*public virtual bool CanCommand(string key)
        {
            bool canUse = false;

            for (int i = 0; i < boundCommands.Length; i++)
            {
                if (boundCommands[i] == key) break;

                if (i >= boundCommands.Length - 1) return false;
            }

            if (commands.CanCommand(key)) canUse = true;
            //if (production.CanCommand(key)) canUse = true;

            return canUse;
        }*/
    }
}