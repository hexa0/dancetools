using System.Collections.Generic;
using DanceTools.Utils;

namespace DanceTools.Commands
{
    internal class TeleportCommand : ICommand
    {
        public string Name => "teleport";
        public string[] Aliases { get { return new string[] { "tp", "tele" }; } }

        public string Desc => "teleports any amount of players to the last specified player in the arguments";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            List<ulong> targets = new List<ulong>();
            var teleportTo = NameUtils.SearchForPlayer(args[args.Length - 1]);

            if (!teleportTo)
            {
                DTConsole.Instance.PushTextToOutput("destination invalid", DanceTools.consoleInfoColor);
                return;
            }

            for (int i = 0; i < args.Length - 1; i++)
            {
                var target = NameUtils.SearchForPlayer(args[i]);

                if (target != null)
                {
                    targets.Add(target.actualClientId);
                }
            }

            NetworkStuff.SendTeleportMessage(teleportTo.transform.position, teleportTo.transform.rotation, targets.ToArray());
        }
    }
}
