using System;
using DanceTools.Utils;

namespace DanceTools.Commands
{
    internal class LightsCommand : ICommand
    {
        public string Name => "lights";
        public string[] Aliases { get { return new string[] { "li" }; } }

        public string Desc => "Toggles lights inside the facility\nUsage: lights on/off";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            if (!DanceTools.CheckCheats()) return;

            if (args.Length < 1)
            {
                DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
                return;
            }

            string color = DanceTools.consoleInfoColor;
            string text = "";
            try
            {
                switch(args[0])
                {
                    case "on":
                        NetworkStuff.SendLightsMessage(true);
                        text = "Indoor lights turned on";
                        break;
                    case "off":
                        NetworkStuff.SendLightsMessage(false);
                        text = "Indoor lights turned off";
                        break;
                }
            } catch (Exception)
            {
                color = DanceTools.consoleErrorColor;
                text = "Failed to toggle lights";
            }

            DTConsole.Instance.PushTextToOutput(text, color);

        }
    }
}
