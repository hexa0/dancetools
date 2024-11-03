namespace DanceTools.Commands
{
    internal class GodCommand : ICommand
    {
        public string Name => "god";
        public string[] Aliases { get { return new string[] { "gd" };  } }

        public string Desc => "Toggles godmode for the host";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            
            if (!DanceTools.CheckCheats()) return;

            //flip flop
            DanceTools.playerGodMode = !DanceTools.playerGodMode;

            string text = DanceTools.playerGodMode ? "God mode enabled" : "God mode disabled";

            DTConsole.Instance.PushTextToOutput(text, DanceTools.consoleInfoColor);
        }
    }
}
