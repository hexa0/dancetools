using DanceTools.Utils;

namespace DanceTools.Commands
{
    internal class CheatsCommand : ICommand
    {
        public string Name => "sv_cheats";
        public string[] Aliases { get { return new string[] { "cheats", "cheat", "ch", "hacks", "hack" }; } }

        public string Desc => "Sets whether cheats should be enabled,\n\t'sv_cheats 1' for enabled\n\t'sv_cheats 0' for disabled";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            if (!DanceTools.CheckHost()) return;

            if (args.Length < 1)
            {
                DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
                return;
            }

            int enabled = DanceTools.CheckInt(args[0]);
            if (enabled == 1) {
                NetworkStuff.SendCheatsToggledMessage(true);
            }
            else
            {
                NetworkStuff.SendCheatsToggledMessage(false);
            }
        }
    }
}