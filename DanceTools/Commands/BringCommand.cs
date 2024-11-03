using System.Collections.Generic;
using DanceTools.Utils;

namespace DanceTools.Commands
{
    internal class BringCommand : ICommand
    {
        public string Name => "bring";
        public string[] Aliases { get { return new string[] { "br", "bringplayer", "bringplr"}; } }

        public string Desc => "teleports the specified player to you";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            List<ulong> targets = new List<ulong>();

            for (int i = 0; i < args.Length; i++)
            {
                var target = NameUtils.SearchForPlayer(args[i]);

                if (target != null)
                {
                    targets.Add(target.actualClientId);
                }
            }

            NetworkStuff.SendTeleportMessage(NetworkStuff.CurrentClient.transform.position, NetworkStuff.CurrentClient.transform.rotation, targets.ToArray());
        }
    }
}
