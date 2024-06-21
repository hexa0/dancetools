namespace DanceTools
{
    public class SpeedCommand : ICommand
    {
        public string Name => "speed";
        public string[] Aliases { get { return new string[] { "sp", "spd", "fast" }; } }

        public string Desc => "toggles the vanilla PlayerControllerB.isSpeedCheating";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            if (!DanceTools.CheckCheats()) { return; }

            GameNetworkManager.Instance.localPlayerController.isSpeedCheating = !GameNetworkManager.Instance.localPlayerController.isSpeedCheating;
            if (GameNetworkManager.Instance.localPlayerController.isSpeedCheating)
            {
                DTConsole.Instance.PushTextToOutput("PlayerControllerB.isSpeedCheating Enabled!", DanceTools.consoleInfoColor);
            }
            else
            {
                DTConsole.Instance.PushTextToOutput("PlayerControllerB.isSpeedCheating Disabled!", DanceTools.consoleInfoColor);
            }

            DTConsole.Instance.ToggleUI();
        }
    }
}