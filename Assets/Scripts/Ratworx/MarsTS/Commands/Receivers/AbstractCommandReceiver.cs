using System;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events;
using Ratworx.MarsTS.Units;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public abstract class AbstractCommandReceiver<T> : MonoBehaviour, 
                                                       ICommandReceiver<T> where T : Commandlet
    {
        public event Action OnCommandStateUpdated;
        public string CommandKey => _commandKey;
        public int EvaluationPriority => _evaluationPriority;
        public bool CanInterrupt => _canInterrupt;

        public abstract bool CanCommand { get; }
        public abstract bool IsActive { get; }
        public abstract float Cooldown { get; }
        public abstract (bool valid, CommandFactory factory) EvaluateCommand(Entity entity);

        protected UnitPathfinder UnitPathing;
        protected UnitTargetManager UnitTargeting;
        protected UnitOwnership Ownership;
        protected CommandQueue CommandQueue;
        protected EventAgent EventAgent;
        
        [SerializeField] private string _commandKey;
        [SerializeField] private int _evaluationPriority;
        [SerializeField] private bool _canInterrupt;
        
        private void Awake() {
            UnitPathing = GetComponent<UnitPathfinder>();
            UnitTargeting = GetComponent<UnitTargetManager>();
            Ownership = GetComponent<UnitOwnership>(); 
            CommandQueue = GetComponent<CommandQueue>();
            EventAgent = GetComponent<EventAgent>();
        }
        
        public abstract void ReceiveCommand(T command);

        protected void PostStateUpdatedEvent() => OnCommandStateUpdated?.Invoke();
    }
}