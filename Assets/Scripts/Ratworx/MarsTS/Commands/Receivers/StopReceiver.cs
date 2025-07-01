using Ratworx.MarsTS.Commands.Commandlets;
using Ratworx.MarsTS.Commands.UI;
using Ratworx.MarsTS.Entities;

namespace Ratworx.MarsTS.Commands.Receivers
{
    public class StopReceiver : AbstractCommandReceiver<BooleanCommandlet>
    {
        public override bool CanCommand => true;
        public override bool IsActive => false;
        public override float Cooldown => 0f;

        public override void ReceiveCommand(BooleanCommandlet command) {
            UnitPathing.ClearPath();
            UnitTargeting.ClearTarget();
            CommandQueue.Clear();
            command.CompleteCommand(CommandQueue);
        }

        // There's no situation this will ever be an auto command
        public override (bool valid, ICommandInterface command) EvaluateCommand(Entity entity) => (false, null);
    }
}