using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Entities;
using Ratworx.MarsTS.Events.Commands;
using Ratworx.MarsTS.Events.Selectable;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class MoveReceiver : AbstractCommandReceiver<MoveCommandlet>
    {
        // TODO: Investigate checking move_speed attribute to determine if we can command (for mobile artillery)
        public override bool CanCommand => true;
        public override bool IsActive => true;
        public override float Cooldown => 0f;
        
        private MoveCommandlet _moveCommand;

        public override void ReceiveCommand(MoveCommandlet command) {
            _moveCommand = command;
            
            UnitPathing.FindPathTo(command.Target);
            EventAgent.AddListener<PathCompleteEvent>(OnPathComplete);
            _moveCommand.Callback.AddListener(OnCommandComplete);
        }

        public override (bool valid, CommandFactory factory) EvaluateCommand(Entity entity)
            => (true, CommandPrimer.Get(CommandKey));

        private void OnCommandComplete(CommandCompleteEvent evnt) {
            EventAgent.RemoveListener<PathCompleteEvent>(OnPathComplete);
            evnt.Command.Callback.RemoveListener(OnCommandComplete);
            _moveCommand = null;
            UnitPathing.ClearPath();
        }

        private void OnPathComplete(PathCompleteEvent evnt) => _moveCommand.CompleteCommand(CommandQueue);
    }
}