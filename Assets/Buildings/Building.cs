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

        public int Id => entityComponent.Id;

        public string UnitType => type;

        public string RegistryKey => "building:" + UnitType;

        public Sprite Icon => icon;

        public Faction Owner => TeamCache.Faction(owner.Value);

        [SerializeField] private Sprite icon;

        [SerializeField] private string type;

        [SerializeField]
        protected NetworkVariable<int> owner =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

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

        [Header("Construction")] [SerializeField]
        protected int constructionWork;

        [SerializeField] protected int currentWork;

        [SerializeField] private GameObject ghost;

        [SerializeField] private CostEntry[] constructionCost;

        public int ConstructionProgress => currentWork;

        public int ConstructionRequired => constructionWork;

        public bool Constructed => currentWork >= constructionWork;

        public GameObject SelectionGhost => ghost;

        public CostEntry[] ConstructionCost => constructionCost;

        protected int healthPerConstructionPoint;

        /*	Upgrades	*/

        protected Entity entityComponent;

        protected EventAgent bus;

        protected Transform model;

        [SerializeField] protected GameObject[] visionObjects;

        protected virtual void Awake()
        {
            bus = GetComponent<EventAgent>();
            entityComponent = GetComponent<Entity>();
            commands = GetComponent<CommandQueue>();
            production = GetComponent<ProductionQueue>();

            healthPerConstructionPoint = Mathf.RoundToInt((float)MaxHealth / constructionWork);

            model = transform.Find("Model");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsServer)
            {
                AttachServerListeners();

                currentHealth.Value = 0;

                if (currentWork > 0)
                {
                    model.localScale = Vector3.one * ((float)currentWork / constructionWork);
                    currentHealth.Value = maxHealth.Value * (currentWork / constructionWork);
                }
                else
                {
                    model.localScale = Vector3.zero;
                }
            }

            if (NetworkManager.Singleton.IsClient) AttachClientListeners();
        }

        protected virtual void AttachServerListeners()
        {
            bus.AddListener<CommandStartEvent>(ExecuteOrder);
        }

        protected virtual void AttachClientListeners()
        {
            bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

            EventBus.AddListener<ResearchCompleteEvent>(OnGlobalResearchComplete);
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);

            owner.OnValueChanged += (_, _) 
                => bus.Local(new UnitOwnerChangeEvent(bus, this, Owner));
        }

        protected virtual void CancelConstruction()
        {
            if (!Constructed)
            {
                bus.Global(new UnitDeathEvent(bus, this));

                foreach (CostEntry materialCost in ConstructionCost)
                {
                    Player.Commander.GetResource(materialCost.key).Deposit(materialCost.amount);
                }

                Destroy(gameObject, 0.1f);
            }
        }

        protected virtual void Upgrade(Commandlet order)
        {
            bus.AddListener<CommandCompleteEvent>(UpgradeComplete);
        }

        protected virtual void UpgradeComplete(CommandCompleteEvent _event)
        {
            bus.RemoveListener<CommandCompleteEvent>(UpgradeComplete);

            IProducable order = _event.Command as IProducable;
            GameObject product = Instantiate(order.Product, transform, false);

            for (int i = 0; i < boundCommands.Length; i++)
                if (boundCommands[i] == _event.Command.Command.Name)
                {
                    boundCommands[i] = "";
                    bus.Global(new CommandsUpdatedEvent(bus, this, Commands()));
                    break;
                }

            bus.Global(new ProductionCompleteEvent(bus, product, this, production, order));
        }

        protected virtual void Research(Commandlet order)
        {
            bus.AddListener<CommandCompleteEvent>(ResearchComplete);
        }

        protected virtual void ResearchComplete(CommandCompleteEvent _event)
        {
            bus.RemoveListener<CommandCompleteEvent>(ResearchComplete);

            IProducable order = _event.Command as IProducable;
            Technology product = Instantiate(order.Product, Owner.transform, false).GetComponent<Technology>();
            Player.SubmitResearch(product);

            for (int i = 0; i < boundCommands.Length; i++)
                if (boundCommands[i] == _event.Command.Command.Name)
                {
                    boundCommands[i] = "";
                    bus.Global(new CommandsUpdatedEvent(bus, this, Commands()));
                    break;
                }

            bus.Global(new ResearchCompleteEvent(bus, product, this, production, order));
            bus.Global(new ProductionCompleteEvent(bus, product.gameObject, this, production, order));
        }

        protected virtual void OnGlobalResearchComplete(ResearchCompleteEvent _event)
        {
            for (int i = 0; i < boundCommands.Length; i++)
                if (_event.CurrentProduction.Get().Command.Name == boundCommands[i])
                    boundCommands[i] = "";
        }

        public string[] Commands()
        {
            if (!Constructed)
                return new[] { "cancelConstruction" };
            return boundCommands;
        }

        public virtual void Order(Commandlet order, bool inclusive)
        {
            if (!GetRelationship(order.Commander).Equals(Relationship.Owned)) return;

            switch (order.Name)
            {
                case "upgrade":
                    production.Enqueue(order);
                    return;
                case "cancelConstruction":
                    CancelConstruction();
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

        public virtual void Select(bool status) => bus.Local(new UnitSelectEvent(bus, status));

        public virtual void Hover(bool status)
        {
            if (Player.Main.HasSelected(this)) return;

            bus.Local(new UnitHoverEvent(bus, status));
        }

        public bool SetOwner(Faction player)
        {
            owner.Value = player.Id;
            return true;
        }

        public virtual void Attack(int damage)
        {
            if (currentWork < constructionWork && damage < 0)
            {
                currentWork -= damage;

                float progress = (float)currentWork / constructionWork;

                Health += healthPerConstructionPoint;
                Health = Mathf.Clamp(Health, 0, MaxHealth);

                model.localScale = Vector3.one * progress;

                bus.Global(new UnitHurtEvent(bus, this));

                if (progress >= 1f) bus.Global(new CommandsUpdatedEvent(bus, this, boundCommands));
                return;
            }

            if (Health <= 0) return;
            if (damage < 0 && Health >= MaxHealth) return;
            Health -= damage;
            bus.Global(new UnitHurtEvent(bus, this));

            if (Health <= 0)
            {
                bus.Global(new UnitDeathEvent(bus, this));
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

            if (!Constructed && key == "cancelConstruction") return true;

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