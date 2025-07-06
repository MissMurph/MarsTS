namespace Ratworx.MarsTS.Commands.Interfaces
{
    public class StopCommandInterface : BaseCommandInterface
    {
        public override void StartSelection() {
            CommandPrimer.GetFactory<CommandFactory<bool>>().ConstructCommand(
                CommandKey,
                true,
                Player.Player.Commander,
                Player.Player.ListSelected,
                Player.Player.Include
            );
        }

        public override void CancelSelection() { }
    }
}