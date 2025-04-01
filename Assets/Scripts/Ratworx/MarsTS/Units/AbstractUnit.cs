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
        ITaggable<AbstractUnit>,
        IAttackable,
        ICommandable
    {
        public GameObject GameObject => gameObject;
        public IUnit Unit => this;

        /*	IAttackable Properties	*/

        public int Health
        {
            get => currentHealth.Value;
            protected set => currentHealth.Value = value;
        }

        public int MaxHealth
        {
            get => maxHealth.Value;
            protected set => currentHealth.Value = value;
        }

        [Header("Health")] [SerializeField] protected NetworkVariable<int> maxHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        [SerializeField] protected NetworkVariable<int> currentHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

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

        public Type Type => typeof(AbstractUnit);

        /*	ICommandable Properties	*/

        public Commandlet CurrentCommand => commands.Current;

        public Commandlet[] CommandQueue => commands.Queue;

        public List<string> Active => commands.Active;

        public List<Timer> Cooldowns => commands.Cooldowns;

        public int Count => commands.Count;

        //protected Queue<Commandlet> commandQueue = new Queue<Commandlet>();

        protected CommandQueue commands;

        [Header("Commands")] [SerializeField] protected string[] boundCommands;

        /*	Unit Fields	*/

        private Entity _entity;

        protected Transform TrackedTarget
        {
            get => _target;
            set
            {
                if (_target != null)
                {
                    EntityCache.TryGet(_target.gameObject.name + ":eventAgent", out EventAgent oldAgent);
                    oldAgent.RemoveListener<UnitDeathEvent>(_event => TrackedTarget = null);
                }

                _target = value;

                if (value != null)
                {
                    EntityCache.TryGet(value.gameObject.name + ":eventAgent", out EventAgent agent);

                    agent.AddListener<UnitDeathEvent>(_event => TrackedTarget = null);

                    SetTarget(value);
                }
            }
        }

        private Transform _target;

        private Vector3 _targetOldPos;

        protected Path CurrentPath { get; set; } = Path.Empty;

        private float _angle;
        protected int PathIndex;

        protected Rigidbody Body;

        private const float MinPathUpdateTime = .5f;
        private const float PathUpdateMoveThreshold = .5f;

        [SerializeField] protected float waypointCompletionDistance;

        protected EventAgent Bus;

        [Header("Vision")] [SerializeField] private GameObject[] hideables;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody>();
            _entity = GetComponent<Entity>();
            Bus = GetComponent<EventAgent>();
            commands = GetComponent<CommandQueue>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(UpdatePath());

                AttachServerListeners();

                if (currentHealth.Value <= 0) currentHealth.Value = maxHealth.Value;
            }

            if (NetworkManager.Singleton.IsClient) AttachClientListeners();

            // TODO: Find a better spot for this, realistically this should be called in the Attach funcs
            currentHealth.OnValueChanged += OnHurt;
        }

        protected void AttachClientListeners()
        {
            EventBus.AddListener<UnitInfoEvent>(OnUnitInfoDisplayed);

            Bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);
            Bus.AddListener<CommandStartEvent>(ExecuteOrder);
        }

        protected void AttachServerListeners()
        {
        }

        protected virtual void Update()
        {
            if (NetworkManager.Singleton.IsServer) ServerUpdate();
            if (NetworkManager.Singleton.IsClient) ClientUpdate();
        }

        protected virtual void ServerUpdate()
        {
        }

        protected virtual void ClientUpdate()
        {
            if (!CurrentPath.IsEmpty)
            {
                Vector3 targetWaypoint = CurrentPath[PathIndex];

                float distance = new Vector3(targetWaypoint.x - transform.position.x, 0,
                    targetWaypoint.z - transform.position.z).magnitude;

                if (distance <= waypointCompletionDistance) PathIndex++;

                if (PathIndex >= CurrentPath.Length)
                {
                    Bus.Local(new PathCompleteEvent(Bus, true));
                    CurrentPath = Path.Empty;
                }
            }
        }

        private void OnPathFound(Path newPath, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                CurrentPath = newPath;
                PathIndex = 0;
            }
        }

        protected void SetTarget(Vector3 _target)
        {
            PathRequestManager.RequestPath(transform.position, _target, OnPathFound);
        }

        protected void SetTarget(Transform _target)
        {
            SetTarget(_target.position);
            this._target = _target;
        }

        protected virtual void Stop()
        {
            CurrentPath = Path.Empty;
            _target = null;

            commands.Clear();

            //CommandCompleteEvent _event = new CommandCompleteEvent(bus, CurrentCommand, false, this);
            //bus.Global(_event);

            //CurrentCommand = null;
        }

        protected virtual void Move(Commandlet order)
        {
            if (order is Commandlet<Vector3> deserialized)
            {
                SetTarget(deserialized.Target);

                Bus.AddListener<PathCompleteEvent>(OnPathComplete);
                order.Callback.AddListener(_event => Bus.RemoveListener<PathCompleteEvent>(OnPathComplete));
            }
        }

        private void OnPathComplete(PathCompleteEvent _event)
        {
            CommandCompleteEvent newEvent = new CommandCompleteEvent(Bus, CurrentCommand, false, this);

            CurrentCommand.CompleteCommand(Bus, this);
        }

        protected IEnumerator UpdatePath()
        {
            if (Time.timeSinceLevelLoad < .5f) yield return new WaitForSeconds(.5f);

            float sqrMoveThreshold = PathUpdateMoveThreshold * PathUpdateMoveThreshold;

            while (true)
            {
                yield return new WaitForSeconds(MinPathUpdateTime);

                if (_target != null && (_target.position - _targetOldPos).sqrMagnitude > sqrMoveThreshold)
                {
                    PathRequestManager.RequestPath(transform.position, _target.position, OnPathFound);
                    _targetOldPos = _target.position;
                }
            }
        }

        public void OnDrawGizmos()
        {
            if (!CurrentPath.IsEmpty)
                for (int i = PathIndex; i < CurrentPath.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(CurrentPath[i], Vector3.one / 2);

                    if (i == PathIndex)
                        Gizmos.DrawLine(transform.position, CurrentPath[i]);
                    else
                        Gizmos.DrawLine(CurrentPath[i - 1], CurrentPath[i]);
                }
        }

        public virtual void Order(Commandlet order, bool inclusive)
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

            if (inclusive) commands.Enqueue(order);
            else commands.Execute(order);
        }

        protected virtual void ExecuteOrder(CommandStartEvent _event)
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
        }

        public AbstractUnit Get() => this;

        public string[] Commands() => boundCommands;

        public abstract CommandFactory Evaluate(ISelectable target);

        public abstract void AutoCommand(ISelectable target);

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

        public void Attack(int damage)
        {
            if (Health <= 0) return;
            if (damage < 0 && Health >= MaxHealth) return;
            
            UnitHurtEvent hurtEvent = new UnitHurtEvent(Bus, this, damage);
            hurtEvent.Phase = Phase.Pre;
            Bus.Global(hurtEvent);

            damage = hurtEvent.Damage;
            Health -= damage;

            hurtEvent.Phase = Phase.Post;
            Bus.Global(hurtEvent);
        }

        protected virtual void OnHurt(int oldHealth, int newHealth)
        {
            if (Health <= 0)
            {
                Bus.Global(new UnitDeathEvent(Bus, this));

                if (NetworkManager.Singleton.IsServer)
                    Destroy(gameObject, 0.1f);
            }
            else
            {
                UnitHurtEvent hurtEvent = new UnitHurtEvent(Bus, this, oldHealth - newHealth);
                hurtEvent.Phase = Phase.Post;
                Bus.Global(hurtEvent);
            }
        }

        protected virtual void OnUnitInfoDisplayed(UnitInfoEvent _event)
        {
            if (ReferenceEquals(_event.Unit, this))
            {
                HealthInfo info = _event.Info.Module<HealthInfo>("health");
                info.CurrentUnit = this;
            }
        }

        protected virtual void OnVisionUpdate(EntityVisibleEvent _event)
        {
            foreach (GameObject hideable in hideables)
            {
                hideable.SetActive(_event.Visible);
            }
        }

        public virtual bool CanCommand(string key)
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
        }
    }
}