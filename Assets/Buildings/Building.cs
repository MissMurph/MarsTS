using System;
using System.Collections.Generic;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Players;
using MarsTS.Research;
using MarsTS.Teams;
using MarsTS.UI;
using MarsTS.Units;
using MarsTS.Vision;
using Unity.Netcode;
using UnityEngine;

namespace MarsTS.Buildings
{
    public abstract class Building : NetworkBehaviour, 
        ISelectable, 
        ITaggable<Building>, 
        IAttackable, 
        ICommandable
    {
        public GameObject GameObject => gameObject;
        public IUnit Unit => this;

        /*	IAttackable Properties	*/

        public virtual int Health
        {
            get => currentHealth.Value;
            protected set => currentHealth.Value = value;
        }

        public virtual int MaxHealth => maxHealth.Value;

        [SerializeField] protected NetworkVariable<int> maxHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        [SerializeField] protected NetworkVariable<int> currentHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        /*	ISelectable Properties	*/

        public int Id => EntityComponent.Id;

        public string UnitType => type;

        public string RegistryKey => "building:" + UnitType;

        public Sprite Icon => icon;

        public Faction Owner => TeamCache.Faction(_owner);

        [SerializeField] private Sprite icon;

        [SerializeField] private string type;

        [SerializeField] protected int _owner;

        /*	ITaggable Properties	*/

        public string Key => "selectable";

        public Type Type => typeof(Building);

        /*	ICommandable Properties	*/

        public Commandlet CurrentCommand => production.Current;

        public Commandlet[] CommandQueue => production.Queue;

        public List<string> Active => commands != null ? commands.Active : new List<string>();

        public List<Timer> Cooldowns => commands != null ? commands.Cooldowns : new List<Timer>();

        public int Count => production.Count;

        protected CommandQueue commands;

        protected ProductionQueue production;

        [SerializeField] protected string[] boundCommands;

        /*	Building Fields	*/

        [Header("Construction")]
        [SerializeField] private GameObject selectionGhost;
        [SerializeField] private GameObject constructionGhost;

        public GameObject SelectionGhost => selectionGhost;
        public GameObject ConstructionGhost => constructionGhost;

        /*	Upgrades	*/

        protected Entity EntityComponent;

        protected EventAgent Bus;

        protected Transform Model;

        [SerializeField] protected GameObject[] visionObjects;

        protected virtual void Awake()
        {
            Bus = GetComponent<EventAgent>();
            EntityComponent = GetComponent<Entity>();
            commands = GetComponent<CommandQueue>();
            production = GetComponent<ProductionQueue>();

            Model = transform.Find("Model");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsServer)
            {
                Health = MaxHealth;
                AttachServerListeners();
            }

            if (NetworkManager.Singleton.IsClient) 
                AttachClientListeners();
        }

        protected virtual void AttachServerListeners() => Bus.AddListener<CommandStartEvent>(ExecuteOrder);

        protected virtual void AttachClientListeners()
        {
            Bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

            EventBus.AddListener<ResearchCompleteEvent>(OnGlobalResearchComplete);
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);
        }

        protected virtual void Upgrade(Commandlet order)
        {
            Bus.AddListener<CommandCompleteEvent>(UpgradeComplete);
        }

        protected virtual void UpgradeComplete(CommandCompleteEvent _event)
        {
            Bus.RemoveListener<CommandCompleteEvent>(UpgradeComplete);

            IProducable order = _event.Command as IProducable;
            GameObject product = Instantiate(order.Product, transform, false);

            for (int i = 0; i < boundCommands.Length; i++)
                if (boundCommands[i] == _event.Command.Command.Name)
                {
                    boundCommands[i] = "";
                    Bus.Global(new CommandsUpdatedEvent(Bus, this, Commands()));
                    break;
                }

            Bus.Global(new ProductionCompleteEvent(Bus, product, this, production, order));
        }

        protected virtual void Research(Commandlet order)
        {
            Bus.AddListener<CommandCompleteEvent>(ResearchComplete);
        }

        protected virtual void ResearchComplete(CommandCompleteEvent _event)
        {
            Bus.RemoveListener<CommandCompleteEvent>(ResearchComplete);

            IProducable order = _event.Command as IProducable;
            Technology product = Instantiate(order.Product, Owner.transform, false).GetComponent<Technology>();
            Player.SubmitResearch(product);

            for (int i = 0; i < boundCommands.Length; i++)
                if (boundCommands[i] == _event.Command.Command.Name)
                {
                    boundCommands[i] = "";
                    Bus.Global(new CommandsUpdatedEvent(Bus, this, Commands()));
                    break;
                }

            Bus.Global(new ResearchCompleteEvent(Bus, product, this, production, order));
            Bus.Global(new ProductionCompleteEvent(Bus, product.gameObject, this, production, order));
        }

        protected virtual void OnGlobalResearchComplete(ResearchCompleteEvent _event)
        {
            for (int i = 0; i < boundCommands.Length; i++)
                if (_event.CurrentProduction.Get().Command.Name == boundCommands[i])
                    boundCommands[i] = "";
        }

        public string[] Commands() => boundCommands;

        public virtual void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            switch (order.Name)
            {
                case "upgrade":
                    production.Enqueue(order);
                    return;
                case "research":
                    production.Enqueue(order);
                    return;
                default:
                    return;
            }
        }

        protected virtual void ExecuteOrder(CommandStartEvent _event)
        {
            switch (_event.Command.Name)
            {
                case "upgrade":
                    Upgrade(_event.Command);
                    break;
                case "research":
                    Research(_event.Command);
                    break;
            }
        }

        public Building Get() => this;

        // TODO: Convert into IAttackable & ISelectable extension method
        public Relationship GetRelationship(Faction other)
        {
            Relationship result = Owner.GetRelationship(other);
            return result;
        }

        public virtual void Select(bool status) => Bus.Local(new UnitSelectEvent(Bus, status));

        public virtual void Hover(bool status)
        {
            if (Player.Main.HasSelected(this)) return;

            Bus.Local(new UnitHoverEvent(Bus, status));
        }

        public bool SetOwner(Faction player)
        {
            if (!NetworkManager.Singleton.IsServer) return false;

            _owner = player.Id;
            SetOwnerClientRpc(_owner);
            Bus.Global(new UnitOwnerChangeEvent(Bus, this, Owner));
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId)
        {
            _owner = newId;
            Bus.Global(new UnitOwnerChangeEvent(Bus, this, Owner));
        }

        public virtual void Attack(int damage)
        {
            if (Health <= 0) 
                return;
            
            if (damage < 0 && Health >= MaxHealth) 
                return;

            UnitHurtEvent hurtEvent = new UnitHurtEvent(Bus, this, damage);
            hurtEvent.Phase = Phase.Pre;
            Bus.Global(hurtEvent);

            damage = hurtEvent.Damage;
            Health -= damage;

            hurtEvent.Phase = Phase.Post;
            Bus.Global(hurtEvent);

            if (Health <= 0)
            {
                Bus.Global(new UnitDeathEvent(Bus, this));
                Destroy(gameObject, 0.1f);
            }
        }

        public virtual CommandFactory Evaluate(ISelectable target) => CommandRegistry.Get("move");

        public virtual void AutoCommand(ISelectable target) { }

        protected virtual void OnUnitInfoDisplayed(UnitInfoEvent @event)
        {
            if (!ReferenceEquals(@event.Unit, this)) return;
            
            HealthInfo info = @event.Info.Module<HealthInfo>("health");
            info.CurrentUnit = this;

            @event.Info.Module<ProductionInfo>("productionQueue").SetQueue(this, production.Current as IProducable,
                production.QueuedProduction);
        }

        protected virtual void OnVisionUpdate(EntityVisibleEvent @event)
        {
            bool visible = @event.Visible | GameVision.WasVisited(gameObject);

            foreach (GameObject hideable in visionObjects)
            {
                hideable.SetActive(visible);
            }
        }

        public bool CanCommand(string key)
        {
            bool canUse = true;

            for (int i = 0; i < boundCommands.Length; i++)
            {
                if (boundCommands[i] == key) break;

                if (i >= boundCommands.Length - 1) return false;
            }

            if (!commands.CanCommand(key)) canUse = false;
            if (!production.CanCommand(key)) canUse = false;

            return canUse;
        }
    }
}