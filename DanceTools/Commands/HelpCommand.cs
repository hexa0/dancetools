﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanceTools.Commands
{
    internal class HelpCommand : ICommand
    {
        //Command that shows list of commands and also description of commands if used as "help cmd"
        public string Name => "help";
        public string[] Aliases { get { return new string[] { "hlp" }; } }

        public string Desc => "Shows list of commands and what each command does";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            string output = "";

            for (int i = 0; i < DanceTools.commands.Count; i++)
            {
                output += $"\n{DanceTools.commands[i].Name}";
                if (DanceTools.commands[i].Aliases != null)
                {
                    foreach (var alias in DanceTools.commands[i].Aliases)
                    {
                        output += $" | {alias}";
                    }
                }
            }

            DTConsole.Instance.PushTextToOutput($"If you need help or have feedback, join the LC Modding discord!\nList Of Commands: \n{output}", DanceTools.consoleSuccessColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            if (args.Length < 1)
            {
                DisplayCommandDesc();
                return;
            }

            bool cmdFound = false;

            for (int i = 0; i < DanceTools.commands.Count; i++)
            {
                var matches = DanceTools.commands[i].Name.ToLower() == args[0].ToLower();
                if (!matches && DanceTools.commands[i].Aliases != null)
                {
                    foreach (var commandAlias in DanceTools.commands[i].Aliases)
                    {
                        if (commandAlias.ToLower() == args[0].ToLower())
                        {
                            matches = true;
                        }
                    }
                }

                if (matches)
                {
                    cmdFound = true;
                    DTConsole.Instance.PushTextToOutput(DanceTools.commands[i].Desc, DanceTools.consoleSuccessColor);
                    break;
                }
            }

            if (!cmdFound)
            {
                DTConsole.Instance.PushTextToOutput($"Command not found", DanceTools.consoleErrorColor);
            }
        }
    }
}
