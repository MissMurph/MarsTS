namespace Ratworx.MarsTS.Commands.UI
{
    public class CancelConstructionUserInterface : BaseCommandUserInterface
    {
        public override void StartSelection() {
            CommandPrimer.GetFactory<CommandFactory<bool>>()
                .ConstructCommand(
                    CommandKey,
                    true,
                    Player.Player.Commander,
                    Player.Player.ListSelected,
                    Player.Player.Include
                );
        }

        public override void CancelSelection() {
        }
    }
}