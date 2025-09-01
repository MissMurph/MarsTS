using System;
using Ratworx.MarsTS.Buildings;
using Ratworx.MarsTS.Commands;
using Ratworx.MarsTS.Commands.Interfaces;
using Ratworx.MarsTS.Commands.Receivers;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Logging;
using UnityEngine;

namespace Ratworx.MarsTS.Units
{
    public class UnitConstructionOptions : MonoBehaviour,
                                           ICommandReceiver
    {
        [SerializeField] private ConstructionOption[] _constructionOptions;
        
        public event Action OnCommandStateUpdated;
        public string CommandKey => "construct";
        public bool CanCommand => true;
        public int EvaluationPriority => 0;
        public bool IsActive => false;
        public bool CanInterrupt => true;
        public float Cooldown => 0f;
        public bool InterruptQueue => false;

        public void ReceiveCommand(Commandlet command) {
            throw new NotSupportedException($"{nameof(UnitConstructionOptions)} cannot receive commands! This should never be reached!");
        }

        public (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);

        public void StartSelection(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error starting {CommandKey} selection, argument is empty!");
                return;
            }

            foreach (ConstructionOption option in _constructionOptions) {
                if (option.OptionKey != argument) continue;
                
                CommandPrimer.GetInterface<ConstructBuildingCommandInterface>(CommandKey).StartArgSelection(option);
                return;
            }
            
            RatLogger.Error?.Log($"Construction option with key {argument} not found!");
        }

        public Sprite GetIcon(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error getting {CommandKey} icon, argument is empty!");
                return null;
            }

            foreach (ConstructionOption option in _constructionOptions) {
                if (option.OptionKey != argument) continue;
                
                return CommandPrimer.GetInterface<ConstructBuildingCommandInterface>(CommandKey).GetArgIcon(option);
            }
            
            RatLogger.Error?.Log($"Construction option with key {argument} not found!");
            return null;
        }

        public string GetDescription(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error getting {CommandKey} description, argument is empty!");
                return string.Empty;
            }

            foreach (ConstructionOption option in _constructionOptions) {
                if (option.OptionKey != argument) continue;
                
                return CommandPrimer.GetInterface<ConstructBuildingCommandInterface>(CommandKey).GetArgDescription(option);
            }
            
            RatLogger.Error?.Log($"Construction option with key {argument} not found!");
            return string.Empty;
        }

        public string GetName(string argument = null) {
            if (string.IsNullOrEmpty(argument)) {
                RatLogger.Error?.Log($"Error getting {CommandKey} name, argument is empty!");
                return string.Empty;
            }

            foreach (ConstructionOption option in _constructionOptions) {
                if (option.OptionKey != argument) continue;
                
                return CommandPrimer.GetInterface<ConstructBuildingCommandInterface>(CommandKey).GetArgDescription(option);
            }
            
            RatLogger.Error?.Log($"Construction option with key {argument} not found!");
            return string.Empty;
        }
    }
}