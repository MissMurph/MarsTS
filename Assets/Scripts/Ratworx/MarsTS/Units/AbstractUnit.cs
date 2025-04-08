using System;
using System.Collections;
using System.Collections.Generic;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;
using Ratworx.MarsTS.Events.Selectable.Attackable;
using Ratworx.MarsTS.Events.Selectable.Internal;
using Ratworx.MarsTS.Pathfinding;
using Ratworx.MarsTS.Teams;
using Ratworx.MarsTS.UI.Unit_Pane;
using Unity.Netcode;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public abstract class AbstractUnit : NetworkBehaviour,
        ISelectable,
        IEntityComponent<AbstractUnit>
    {
        public GameObject GameObject => gameObject;
        public IUnitInterface UnitInterface => this;

        /*	ISelectable Properties	*/

        public int Id => _entity.Id;

        public string UnitType => type;

        public string RegistryKey => "unit:" + UnitType;

        public Sprite Icon => icon;

        public Faction Owner => TeamCache.Faction(owner);

        [Header("Unit Details")] [SerializeField]
        private Sprite icon;

        [SerializeField] private string type;

        [SerializeField] protected int owner;

        /*	ITaggable Properties	*/

        public string Key => "selectable";
        
        /*	Unit Fields	*/

        private Entity _entity;

        protected Rigidbody Body;
        
        protected EventAgent Bus;

        [Header("Vision")] [SerializeField] private GameObject[] hideables;

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

            Bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);
            // Bus.AddListener<CommandStartEvent>(ExecuteOrder);
        }
        

        protected virtual void Stop()
        {
            // CurrentPath = Path.Empty;
            // _target = null;

            // commands.Clear();

            //CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, false, this);
            //bus.Global(_event);

            //CurrentCommand = null;
        }

        protected virtual void Move(Commandlet order)
        {
            if (order is Commandlet<Vector3> deserialized)
            {
                // SetTarget(deserialized.Target);

                Bus.AddListener<PathCompleteEvent>(OnPathComplete);
                order.Callback.AddListener(_event => Bus.RemoveListener<PathCompleteEvent>(OnPathComplete));
            }
        }

        private void OnPathComplete(PathCompleteEvent _event)
        {
            // CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);
            //
            // CurrentCommand.CompleteCommand(Bus, this);
        }

        

        

        /*public virtual void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            switch (order.Name)
            {
                case "move":
                    break;
                case "stop":
                    break;
                default:
                    return;
            }

            if (inclusive) commands.EnqueueCommand(order);
            else commands.ExecuteCommand(order);
        }*/

        /*protected virtual void ExecuteOrder(CommandStartEvent _event)
        {
            switch (_event.Command.Name)
            {
                case "move":
                    Move(_event.Command);
                    break;
                case "stop":
                    Stop();
                    break;
            }
        }*/

        public AbstractUnit Get() => this;

        public virtual void Select(bool status)
        {
            //selectionCircle.SetActive(status);
            Bus.Local(new UnitSelectEvent(Bus, status));
        }

        public virtual void Hover(bool status)
        {
            //These are seperated due to the Player Selection Check
            if (status)
                //selectionCircle.SetActive(true);
                Bus.Local(new UnitHoverEvent(Bus, status));
            else if (!Player.Player.HasSelected(this))
                //selectionCircle.SetActive(false);
                Bus.Local(new UnitHoverEvent(Bus, status));
        }

        public Relationship GetRelationship(Faction other) => Owner.GetRelationship(other);

        public bool SetOwner(Faction player)
        {
            if (!NetworkManager.Singleton.IsServer) return false;

            owner = player.Id;
            SetOwnerClientRpc(owner);
            Bus.Global(new UnitOwnerChangeEvent(Bus, this, Owner));
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId)
        {
            owner = newId;
            Bus.Global(new UnitOwnerChangeEvent(Bus, this, Owner));
        }

        protected virtual void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                HealthInfo info = _event.Info.Module<HealthInfo>("health");
                // info.CurrentUnit = this;
            }
        }

        protected virtual void OnVisionUpdate(EntityVisibleEvent _event)
        {
            foreach (GameObject hideable in hideables)
            {
                hideable.SetActive(_event.Visible);
            }
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