namespace DanceTools.Commands
{
    internal class CloseConsole : ICommand
    {
        public string Name => "close";
        public string[] Aliases { get { return new string[] { "exit" }; } }

        public string Desc => "Closes the console in case of bug/can't close it";

        public bool AutocloseUI => true;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {

        }
    }
}
