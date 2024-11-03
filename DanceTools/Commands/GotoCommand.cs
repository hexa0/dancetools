using System.Collections.Generic;
using DanceTools.Utils;

namespace DanceTools.Commands
{
    internal class GotoCommand : ICommand
    {
        public string Name => "goto";
        public string[] Aliases { get { return new string[] { "go", "teleto", "teleporto"}; } }

        public string Desc => "teleports you to the specified player";

        public bool AutocloseUI => true;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            if (args[0] == null)
            {
                DTConsole.Instance.PushTextToOutput("no destination", DanceTools.consoleInfoColor);
                return;
            }

            var teleportTo = NameUtils.SearchForPlayer(args[0]);

            if (!teleportTo)
            {
                DTConsole.Instance.PushTextToOutput("destination invalid", DanceTools.consoleInfoColor);
                return;
            }

            ulong[] targets = { NetworkStuff.CurrentClient.actualClientId };

            NetworkStuff.SendTeleportMessage(teleportTo.transform.position, teleportTo.transform.rotation, targets);
        }
    }
}
