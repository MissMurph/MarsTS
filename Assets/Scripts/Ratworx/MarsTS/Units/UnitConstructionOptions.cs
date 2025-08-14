using System;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Entities;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class UnitConstructionOptions : MonoBehaviour,
                                           ICommandReceiver
    {
        public event Action OnCommandStateUpdated;
        public string CommandKey => "construct";
        public bool CanCommand => true;
        public int EvaluationPriority => 0;
        public bool IsActive => false;
        public bool CanInterrupt => true;
        public float Cooldown => 0f;
        public void ReceiveCommand(Commandlet command) {
            throw new NotImplementedException($"{nameof(UnitConstructionOptions)} cannot receive commands! This should never be reached!");
        }

        public (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);

        public void StartSelection(string argument = null) {
            throw new NotImplementedException();
        }

        public Sprite GetIcon(string argument = null) => throw new NotImplementedException();

        public string GetDescription(string argument = null) => throw new NotImplementedException();
    }
}