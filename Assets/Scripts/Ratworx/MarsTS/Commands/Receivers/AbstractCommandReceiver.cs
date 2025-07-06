using System;
using Ratworx.MarsTS.Commands.Interfaces;
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

        protected UnitPathfinder UnitPathing;
        protected UnitTargetManager UnitTargeting;
        protected UnitOwnership Ownership;
        protected CommandQueue CommandQueue;
        protected EventAgent EventAgent;
        
        [SerializeField] private string _commandKey;
        [SerializeField] private int _evaluationPriority;
        [SerializeField] private bool _canInterrupt;
        
        protected virtual void Awake() {
            UnitPathing = GetComponentInParent<UnitPathfinder>();
            UnitTargeting = GetComponentInParent<UnitTargetManager>();
            Ownership = GetComponentInParent<UnitOwnership>(); 
            CommandQueue = GetComponentInParent<CommandQueue>();
            EventAgent = GetComponentInParent<EventAgent>();
        }

        public void ReceiveCommand(Commandlet command) {
            if (command.Name == CommandKey
                && command is T superType) 
                ReceiveCommand(superType);
        }
        public abstract void ReceiveCommand(T command);
        public abstract (bool valid, ICommandInterface command) EvaluateCommand(Entity entity);
        protected void PostStateUpdatedEvent() => OnCommandStateUpdated?.Invoke();
        public virtual void StartSelection(string argument = null) 
            => CommandPrimer.GetInterface(CommandKey).StartSelection();
        public virtual Sprite GetIcon(string argument = null) 
            => CommandPrimer.GetInterface(CommandKey).GetIcon();
        public virtual string GetDescription(string argument = null) 
            => CommandPrimer.GetInterface(CommandKey).Description;
    }
}