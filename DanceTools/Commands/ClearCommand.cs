using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceTools.Commands
{
    internal class ClearCommand : ICommand
    {
        public string Name => "clear";
        public string[] Aliases { get { return new string[] { "cls", "clr" }; } }

        public string Desc => "Clears the console log";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            DTConsole.Instance.ClearConsole();
        }
    }
}
