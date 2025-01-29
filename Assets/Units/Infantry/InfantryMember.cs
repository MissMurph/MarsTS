using System;
using System.Collections;
using System.Collections.Generic;
using MarsTS.Commands;
using MarsTS.Entities;
using MarsTS.Events;
using MarsTS.Teams;
using MarsTS.World.Pathfinding;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace MarsTS.Units
{
    public class InfantryMember : NetworkBehaviour,
        ISelectable,
        ITaggable<InfantryMember>,
        IAttackable,
        ICommandable
    {
        public GameObject GameObject => gameObject;
        public IUnit Unit => this;

        /*	IAttackable Properties	*/

        public int Health
        {
            get => _currentHealth.Value;
            private set => _currentHealth.Value = value;
        }

        public int MaxHealth
        {
            get => _maxHealth.Value;
            private set => _maxHealth.Value = value;
        }

        [FormerlySerializedAs("maxHealth")]
        [SerializeField]
        protected NetworkVariable<int> _maxHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        [FormerlySerializedAs("currentHealth")]
        [SerializeField]
        protected NetworkVariable<int> _currentHealth =
            new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Server);

        /*	ISelectable Properties	*/

        public int Id => _entityComponent.Id;

        public string UnitType => _type;

        public string RegistryKey => $"unit:{UnitType}";

        public Sprite Icon => _icon;

        public Faction Owner => _squad?.Owner ?? _owner;

        [FormerlySerializedAs("icon")]
        [SerializeField]
        private Sprite _icon;

        [FormerlySerializedAs("type")]
        [SerializeField]
        private string _type;

        [FormerlySerializedAs("owner")]
        [SerializeField]
        protected Faction _owner;

        /*	ITaggable Properties	*/

        public string Key => "selectable";

        public Type Type => typeof(AbstractUnit);

        /*	ICommandable Properties	*/

        public Commandlet CurrentCommand => _squad?.CurrentCommand ?? _commandQueueComponent.Current;

        public Commandlet[] CommandQueue => _squad?.CommandQueue ?? _commandQueueComponent.Queue;

        public List<string> Active => _squad?.Active ?? _commandQueueComponent.Active;

        public List<Timer> Cooldowns => _squad?.Cooldowns ?? _commandQueueComponent.Cooldowns;

        public int Count => _squad?.Count ?? _commandQueueComponent.Count;

        /*	Infantry Fields	*/

        protected Entity _entityComponent;

        protected CommandQueue _commandQueueComponent;

        [SerializeField]
        protected InfantrySquad _squad;

        [FormerlySerializedAs("moveSpeed")]
        [SerializeField]
        protected float _moveSpeed;

        protected float _currentSpeed;

        private GroundDetection _ground;

        private bool _isSelected;

        protected Transform TrackedTarget
        {
            get => _target;
            set
            {
                if (_target != null)
                {
                    EntityCache.TryGet(_target.gameObject.name + ":eventAgent", out EventAgent oldAgent);
                    oldAgent.RemoveListener<UnitDeathEvent>(_ => TrackedTarget = null);
                }

                _target = value;

                if (value != null)
                {
                    EntityCache.TryGet(value.gameObject.name + ":eventAgent", out EventAgent agent);

                    agent.AddListener<UnitDeathEvent>(_ => TrackedTarget = null);

                    SetTarget(value);
                }
            }
        }

        private Transform _target;

        private Vector3 _targetOldPos;

        protected Path _currentPath { get; set; } = Path.Empty;

        private float _angle;
        private int _pathIndex;

        private Rigidbody _rigidBody;

        private const float MinPathUpdateTime = .5f;
        private const float PathUpdateMoveThreshold = .5f;

        [FormerlySerializedAs("waypointCompletionDistance")]
        [SerializeField]
        protected float _waypointCompletionDistance;

        protected EventAgent _bus;

        [FormerlySerializedAs("hideables")]
        [SerializeField]
        private GameObject[] _hideables;

        protected virtual void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            _entityComponent = GetComponent<Entity>();
            _bus = GetComponent<EventAgent>();
            _ground = GetComponent<GroundDetection>();
            _commandQueueComponent = GetComponent<CommandQueue>();

            _isSelected = false;
        }

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                _currentHealth = _maxHealth;
                _currentSpeed = _moveSpeed;
            }

            if (NetworkManager.Singleton.IsClient) _bus.AddListener<EntityVisibleEvent>(OnVisionUpdate);

            StartCoroutine(UpdatePath());

            // TODO: Find a better spot for this, realistically this should be called in the Attach funcs
            _currentHealth.OnValueChanged += OnHurt;
        }

        protected virtual void Update()
        {
            if (!_currentPath.IsEmpty)
            {
                Vector3 targetWaypoint = _currentPath[_pathIndex];
                float distance = new Vector3(targetWaypoint.x - transform.position.x, 0,
                    targetWaypoint.z - transform.position.z).magnitude;

                if (distance <= _waypointCompletionDistance) _pathIndex++;

                if (_pathIndex >= _currentPath.Length)
                {
                    _bus.Local(new PathCompleteEvent(_bus, true));
                    _currentPath = Path.Empty;
                }
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!NetworkManager.Singleton.IsServer) return;

            if (_ground.Grounded)
            {
                //Dunno why we need this check on the infantry member when we don't need it on any other unit type...
                if (!_currentPath.IsEmpty && !(_pathIndex >= _currentPath.Length))
                {
                    Vector3 targetWaypoint = _currentPath[_pathIndex];

                    Vector3 targetDirection = new Vector3(targetWaypoint.x - transform.position.x, 0,
                        targetWaypoint.z - transform.position.z).normalized;
                    float targetAngle = Mathf.Atan2(-targetDirection.z, targetDirection.x) * Mathf.Rad2Deg + 90f;
                    _rigidBody.MoveRotation(Quaternion.Euler(transform.eulerAngles.x, targetAngle,
                        transform.eulerAngles.z));

                    Vector3 moveDirection = Vector3.ProjectOnPlane(transform.forward, _ground.Slope.normal);

                    Vector3 newVelocity = moveDirection * _currentSpeed;

                    _rigidBody.velocity = newVelocity;
                }
                else
                {
                    _rigidBody.velocity = Vector3.zero;
                }
            }
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

        public void SetSquad(InfantrySquad squad)
        {
            _squad = squad;
            SetSquadClientRpc(squad.name);
        }

        [Rpc(SendTo.NotServer)]
        private void SetSquadClientRpc(string squadEntityName)
        {
            if (!EntityCache.TryGet(squadEntityName, out InfantrySquad squad))
            {
                Debug.LogError($"Couldn't find {typeof(InfantrySquad)} with name {squadEntityName}!");
                return;
            }

            _squad = squad;
        }

        public void OnDrawGizmos()
        {
            if (!_currentPath.IsEmpty)
                for (int i = _pathIndex; i < _currentPath.Length; i++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(_currentPath[i], Vector3.one / 2);

                    if (i == _pathIndex)
                        Gizmos.DrawLine(transform.position, _currentPath[i]);
                    else
                        Gizmos.DrawLine(_currentPath[i - 1], _currentPath[i]);
                }
        }

        private void OnPathFound(Path newPath, bool pathSuccessful)
        {
            if (pathSuccessful)
            {
                _currentPath = newPath;
                _pathIndex = 0;
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
            _currentPath = Path.Empty;
            _target = null;
        }

        public virtual void Order(Commandlet order, bool inclusive)
        {
            switch (order.Name)
            {
                case "move":
                    Move(order);
                    break;
                case "stop":
                    Stop();
                    break;
            }
        }

        /*	Commands	*/

        protected virtual void Move(Commandlet order)
        {
            if (order is Commandlet<Vector3> deserialized)
            {
                SetTarget(deserialized.Target);

                _bus.AddListener<PathCompleteEvent>(OnPathComplete);
                order.Callback.AddListener(_event => _bus.RemoveListener<PathCompleteEvent>(OnPathComplete));
            }
        }

        private void OnPathComplete(PathCompleteEvent _event)
        {
            CommandCompleteEvent newEvent = new CommandCompleteEvent(_bus, CurrentCommand, false, this);

            CurrentCommand.Callback.Invoke(newEvent);
        }

        public virtual void Select(bool status)
        {
            _bus.Local(new UnitSelectEvent(_bus, status));
            _isSelected = status;
        }

        public virtual void Hover(bool status)
        {
            //These are seperated due to the Player Selection Check
            if (status)
                _bus.Local(new UnitHoverEvent(_bus, status));
            else if (!_isSelected)
                _bus.Local(new UnitHoverEvent(_bus, status));
        }

        public virtual void AutoCommand(ISelectable target)
        {
            throw new NotImplementedException();
        }

        public virtual CommandFactory Evaluate(ISelectable target) => throw new NotImplementedException();

        public Relationship GetRelationship(Faction other) => Owner.GetRelationship(other);

        public bool SetOwner(Faction player)
        {
            if (!NetworkManager.Singleton.IsServer) return false;

            _owner = player;
            SetOwnerClientRpc(_owner.Id);
            _bus.Global(new UnitOwnerChangeEvent(_bus, this, Owner));
            return true;
        }

        [Rpc(SendTo.NotServer)]
        private void SetOwnerClientRpc(int newId)
        {
            _owner = TeamCache.Faction(newId);
            _bus.Global(new UnitOwnerChangeEvent(_bus, this, Owner));
        }

        public void Attack(int damage)
        {
            if (Health <= 0)
                return;

            if (damage < 0 && Health >= MaxHealth)
                return;

            UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, damage);
            hurtEvent.Phase = Phase.Pre;
            _bus.Global(hurtEvent);

            damage = hurtEvent.Damage;
            Health -= damage;

            hurtEvent.Phase = Phase.Post;
            _bus.Global(hurtEvent);
        }

        protected virtual void OnHurt(int oldHealth, int newHealth)
        {
            if (Health <= 0)
            {
                _bus.Global(new UnitDeathEvent(_bus, this));

                if (NetworkManager.Singleton.IsServer)
                    Destroy(gameObject, 0.1f);
            }
            else
            {
                UnitHurtEvent hurtEvent = new UnitHurtEvent(_bus, this, oldHealth - newHealth);
                hurtEvent.Phase = Phase.Post;
                _bus.Global(hurtEvent);
            }
        }

        public InfantryMember Get() => this;

        public string[] Commands() => new string[0];

        protected virtual void OnVisionUpdate(EntityVisibleEvent _event)
        {
            foreach (GameObject hideable in _hideables)
            {
                hideable.SetActive(_event.Visible);
            }
        }

        public bool CanCommand(string key) => throw new NotImplementedException();
    }
}