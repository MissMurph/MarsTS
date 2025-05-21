using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Selectable;
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
    }
}