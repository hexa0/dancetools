using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace DanceTools
{
    //logic of the console
    public class DTCmdHandler : MonoBehaviour
    {
        public static DTCmdHandler Instance;

        //sanity check
        private void Awake()
        {
            Instance = this;
            DanceTools.mls.LogInfo("Loading Commands..");

            var interfaceType = typeof(ICommand);
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var cmdTypes = types.Where(t => interfaceType.IsAssignableFrom(t) && t.IsClass);

            foreach (var cmd in cmdTypes)
            {
                var inst = (ICommand)Activator.CreateInstance(cmd);
                DanceTools.commands.Add(inst);
                DanceTools.mls.LogInfo($"Loaded {inst.Name} command!");
            }
            DanceTools.mls.LogInfo("Commands Loaded!");
        }

        //check if we have the command
        public void CheckCommand(string input)
        {
            //it's a console, we dont need to check for prefixes
            string[] args = input.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries); // <- fix for empty spaces counting as elements

            //if nothing, ignore it
            if (args.Length == 0) return;

            bool cmdFound = false;

            for (int i = 0; i < DanceTools.commands.Count; i++) 
            {
                var usedAlias = DanceTools.commands[i].Name;
                var matches = DanceTools.commands[i].Name.ToLower() == args[0].ToLower();
                if (!matches && DanceTools.commands[i].Aliases != null)
                {
                    foreach (var alias in DanceTools.commands[i].Aliases)
                    {
                        if (alias.ToLower() == args[0].ToLower())
                        {
                            usedAlias = alias;
                            matches = true;
                        }
                    }
                }

                if (matches)
                {
                    cmdFound = true;
                    TriggerCommand(DanceTools.commands[i], args.Skip(1).ToArray(), usedAlias);
                    break;
                }
            }
            if(!cmdFound)
            {
                DTConsole.Instance.PushTextToOutput($"Invalid Command", DanceTools.consoleErrorColor);
            }
            //if (commands.Contains(msg[0].ToLower()))
        }
        public void TriggerCommand(ICommand cmd, string[] args, string alias)
        {
            cmd.ExecCommand(args, alias);
            if (cmd.AutocloseUI)
            {
                DTConsole.Instance.ToggleUI();
            }
        }

    }

    //handles inputs, outputs and keybind to the console
    public class DTConsole : MonoBehaviour
    {
        //ui things
        internal static bool isUIOpen = true; //
        public GameObject holder;
        public Image inputBackground;
        public Image outputBackground;
        public TMP_InputField input;
        public TextMeshProUGUI output;
        public ScrollRect scroll;
        private string oldOutput = "";
        public static DTConsole Instance;
        internal static string[] sillyMessages = 
        { 
            "Hey there!",
            "Console colors are customizable in the config o.o",
            "Haiii! >.<",
            "Dancing on the moon or something..",
            "If you need help or have feedback, join the LC Modding discord!"
        };
        public List<string> history = new List<string>();
        private int historyIndex = 0;

        private void Awake()
        {
            Instance = this;
            //get ui elements from asset bundle
            holder = transform.Find("Holder").gameObject;
            input = transform.Find("Holder/InputBackground/InputField").GetComponent<TMP_InputField>();
            output = transform.Find("Holder/OutputBackground/Scroll/Viewport/OutputField").GetComponent<TextMeshProUGUI>();
            scroll = transform.Find("Holder/OutputBackground/Scroll").GetComponent<ScrollRect>();

            DanceTools.mls.LogInfo($"Setup holder: {holder.name}");
            DanceTools.mls.LogInfo($"Setup input: {input.name}");
            DanceTools.mls.LogInfo($"Setup output: {output.name}");

            input.onSubmit.AddListener(text => { OnEditEnd(text); }); ; //worky :^]
            //set default starting command to help
            input.text = "help";
            //clear console
            output.text = "";
            //intro message
            PushTextToOutput($"\n{sillyMessages[UnityEngine.Random.Range(0, sillyMessages.Length)]}\nDanceTools v{DanceTools.pluginVersion}\n", "#FF00FF");

            //hide the console on startup
            holder.SetActive(false); //uncomment
        }

        public void SetCustomizationSettings()
        {
            inputBackground = transform.Find("Holder/InputBackground").GetComponent<Image>();
            outputBackground = transform.Find("Holder/OutputBackground").GetComponent<Image>();
            //set the background window colors
            //outputBackground.color = new Color(0f, 0f, 0f, DanceTools.consoleOutputFieldOpacity);
            //inputBackground.color = new Color(0f, 0f, 0f, DanceTools.consoleInputFieldOpacity);

            outputBackground.color = DanceTools.consoleOutputFieldColor;
            inputBackground.color = DanceTools.consoleInputFieldColor;

            //PushTextToOutput($"{DanceTools.consoleInputFieldColor}");
            
        }

        //User input
        public void OnEditEnd(string txt)
        {
            PushTextToOutput($"c{NetworkStuff.CurrentClient.actualClientId}@dancetools ~> {input.text}", DanceTools.consolePlayerColor); 
            if (history.Count >= 1 && history[history.Count - 1] != input.text)
            {
                history.Add(input.text);
            }
            else if (history.Count < 1)
            {
                history.Add(input.text);
            }

            historyIndex = history.Count;

            //do stuff with input.text
            DTCmdHandler.Instance.CheckCommand(input.text);
            //...
            input.text = "";
            input.ActivateInputField();
        }
        //Every response sent back
        public void PushTextToOutput(string text, string color = "#92d9f7")
        {
            if (scroll.normalizedPosition.y <= 0.01f) // we're already scrolled all the way down, scroll back down on the next frame
            {
                StartCoroutine(ScrollToBottom());
            }

            output.text = $"{oldOutput}\n<color={color}>{text}</color>";
            oldOutput = output.text;
        }

        public IEnumerator ScrollToBottom()
        {
            yield return 0;
            scroll.normalizedPosition = new Vector2(0, 0);
        }

        //ui key
        public void Update()
        {
            //if (!DanceTools.isHost) return; //ignore if not host
            if (DanceTools.keyboardShortcut.IsDown())
            {
                ToggleUI();
            }

            var direction = 0;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                direction = 1;
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                direction = -1;
            }

            if (direction != 0)
            {
                // DTConsole.Instance.PushTextToOutput(historyIndex.ToString());
                // DTConsole.Instance.PushTextToOutput(history.Count.ToString());

                historyIndex = Mathf.Clamp(historyIndex - direction, 0, history.Count);

                if (historyIndex == history.Count)
                {
                    input.text = "";
                }
                else
                {
                    if (history[historyIndex] != null)
                    {
                        input.text = history[historyIndex];
                        input.caretPosition = input.text.Length;
                    }
                }
            }
        }

        //toggle ui;
        public void ToggleUI()
        {
            isUIOpen = !isUIOpen;
            
            if (isUIOpen)
            {
                holder.gameObject.SetActive(false);
                if (DanceTools.isIngame)
                {
                    //for when the player is in the main menu. weird case, otherwise it gets stuck
                    GameNetworkManager.Instance.localPlayerController.quickMenuManager.isMenuOpen = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
            else
            {
                holder.gameObject.SetActive(true);
                if (DanceTools.isIngame)
                {
                    GameNetworkManager.Instance.localPlayerController.quickMenuManager.isMenuOpen = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                //clear console on open if config is set
                if (DanceTools.consoleClearAfterOpening)
                {
                    ClearConsole();
                }

                //auto focus and reset text to nothing
                input.text = "";
                input.ActivateInputField();

            }
        }
        public void ClearConsole()
        {
            output.text = "";
            oldOutput = "";
        }

    }

}
