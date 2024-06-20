using System.Linq;
using UnityEngine;

namespace DanceTools.Commands
{
    internal class ItemCommand : ICommand
    {
        public string Name => "item";
        public string[] Aliases { get { return new string[] { "itm", "i" }; } }

        public string Desc => "Spawns items on you\nUsage: item itemID/itemName amount value weight\nType just the command without arguments \nto see list of items";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        //args string array doesn't include the original command
        //ie: if you type "item 50 3 1"
        //you will only receive "50 3 1" in args
        public void ExecCommand(string[] args, string alias)
        {
            if (!DanceTools.CheckCheats()) return;

            if (args.Length < 1)
            {
                string itemPrint = $"\nItem List (ID | Name) Total items: {DanceTools.spawnableItems.Count}";

                for (int i = 0; i < DanceTools.spawnableItems.Count; i++)
                {
                    itemPrint += $"\n{DanceTools.spawnableItems[i].id} | {DanceTools.spawnableItems[i].name}";
                }

                DTConsole.Instance.PushTextToOutput($"{itemPrint}", DanceTools.consoleSuccessColor);
                DTConsole.Instance.PushTextToOutput("Command usage: item itemID/itemName amount value weight", DanceTools.consoleInfoColor);
                return;
            }

            int value = 1;
            int amount = 1;
            int index = 0;
            float weight = -1f;

            //both id and name search support
            if (int.TryParse(args[0], out int val))
            {
                //if it is id
                index = val;
            }
            else
            {
                //if it is item name
                if(!DanceTools.spawnableItems.Any((x) => x.name.ToLower().Contains(args[0])))
                {
                    DTConsole.Instance.PushTextToOutput($"Cannot find item by the name: {args[0]}", DanceTools.consoleErrorColor);
                    return;
                }

                index = DanceTools.spawnableItems.Find((x) => x.name.ToLower().Contains(args[0])).id;
            }

            //StartOfRound.Instance.allItemsList.itemsList.Find((x) => x.name.ToLower().Contains(itemName);

            //check if item is in the AllItemsList, if not, ignore it
            if (index > StartOfRound.Instance.allItemsList.itemsList.Count || index < 0)
            {
                DTConsole.Instance.PushTextToOutput($"Invalid Item ID: {index}", DanceTools.consoleErrorColor);
                return;
            }
            //item (id amount value)
            if (args.Length > 1)
            {
                //fix for invalid args
                amount = DanceTools.CheckInt(args[1]);
                if (amount == -1) return;

                if (amount <= 0)
                {
                    DTConsole.Instance.PushTextToOutput($"Amount cannot be 0 or less than 0", DanceTools.consoleErrorColor);
                    return;
                }
            }
            //GameNetworkManager.Instance.localPlayerController.transform.position
            
            Vector3 spawnPos = GameNetworkManager.Instance.localPlayerController.transform.position;

            if(GameNetworkManager.Instance.localPlayerController.isPlayerDead)
            {
                spawnPos = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.transform.position;
                DTConsole.Instance.PushTextToOutput($"Spawning item on {GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.playerUsername}", DanceTools.consoleInfoColor);
            }

            //item modifiers

            //set cost for item
            if (args.Length > 2)
            {
                value = DanceTools.CheckInt(args[2]);
                //fix for invalid args
                if (value == -1)
                {
                    value = 1;
                }
            }

            //set weight if applicable
            if (args.Length > 3)
            {
                weight = DanceTools.CheckFloat(args[3]);
                //fix for invalid args
                if (weight == -1f)
                {
                    weight = -1f;
                }
            }

            NetworkStuff.SendItemMessage(new NetworkStuff.SerializableItemSpawnData(index, spawnPos, (uint)amount, weight, value));
        }
    }
}
