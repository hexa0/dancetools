namespace DanceTools
{
    public class NoclipCommand : ICommand
    {
        public string Name => "noclip";
        public string[] Aliases { get { return new string[] { "nc", "clip" }; } }

        public string Desc => "toggle player noclip";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            if (!DanceTools.CheckCheats()) { return; }

            DanceTools.playerNoclipping = !DanceTools.playerNoclipping;

            var controller = GameNetworkManager.Instance.localPlayerController;

            if (DanceTools.playerNoclipping)
            {
                if (!controller.gameObject.GetComponent<NoclipBehaviour>())
                {
                    controller.gameObject.AddComponent<NoclipBehaviour>();
                }

                DTConsole.Instance.PushTextToOutput("Noclip Enabled!", DanceTools.consoleInfoColor);
            }
            else
            {
                DTConsole.Instance.PushTextToOutput("Noclip Disabled!", DanceTools.consoleInfoColor);
            }

            DTConsole.Instance.ToggleUI();
        }
    }
}