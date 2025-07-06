using System;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public interface ICommandReceiver<T> : ICommandReceiver where T : Commandlet
    {
        void ReceiveCommand(T command);
    }
    
    public interface ICommandReceiver
    {
        event Action OnCommandStateUpdated;
        string CommandKey { get; }
        bool CanCommand { get; }
        int EvaluationPriority { get; }
        bool IsActive { get; }
        /// <summary>If true, this command cannot be cancelled or stopped until completion.</summary>
        bool CanInterrupt { get; }
        /// <remarks>Will return <c>0</c> if no cooldown.</remarks>
        float Cooldown { get; }
        void ReceiveCommand(Commandlet command);
        /// <summary>Evaluates if a command can automatically be determined and constructed with the given
        /// <see cref="Entity"/> as a target.</summary>
        /// <returns>Valid determines if the target is a valid candidate for this command. Factory will be null if valid
        /// is false.</returns>
        (bool valid, ICommandInterface command) EvaluateCommand(Entity entity);
        // TODO: Find better home for below
        void StartSelection(string argument = null);
        Sprite GetIcon(string argument = null);
        string GetDescription(string argument = null);
    }
}