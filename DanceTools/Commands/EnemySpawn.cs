﻿using System;
using System.Linq;
using UnityEngine;
using DanceTools.Utils;

namespace DanceTools.Commands
{
    internal class EnemySpawn : ICommand
    {
        public string Name => "enemy";
        public string[] Aliases { get { return new string[] { "e", "monster" }; } }

        public string Desc => "Spawns enemies\nUsage: enemy name amount (onme/inside/outside)\nType just the command without arguments \nto see list of enemies";

        public bool AutocloseUI => false;

        public void DisplayCommandDesc()
        {
            DTConsole.Instance.PushTextToOutput(Desc, DanceTools.consoleInfoColor);
        }

        public void ExecCommand(string[] args, string alias)
        {
            if (!DanceTools.CheckCheats()) return;

            //if first arguemnt doesnt exist, show how to use command
            if (args.Length < 1)
            {
                string consoleInfo = "\nSpawnable Enemies (Name | Inside/Outside)\n<color=red>Warning. Enemies spawning in wrong place may break the game\nSpawn at your own risk.</color>";

                if (DanceTools.currentRound.currentLevel.Enemies.Count <= 0)
                {
                    DTConsole.Instance.PushTextToOutput($"No enemies to spawn in this level", DanceTools.consoleErrorColor);
                    return;
                }

                for (int i = 0; i < DanceTools.spawnableEnemies.Count; i++)
                {
                    consoleInfo += $"\n{i} | {DanceTools.spawnableEnemies[i].name} | {(DanceTools.spawnableEnemies[i].isOutside ? "outside" : "inside")}";
                }
                //DanceTools.currentRound.currentLevel.OutsideEnemies
                //DanceTools.currentRound.outsideAINodes <- spawnplace
                DTConsole.Instance.PushTextToOutput($"{consoleInfo}", DanceTools.consoleSuccessColor);
                DTConsole.Instance.PushTextToOutput("Command usage: enemy name amount (onme/inside/outside)", DanceTools.consoleInfoColor);
                return;
            }
            //try the command if it has more than 1 argument
            try
            {
                //vars

                int enemyIndex = 0;
                string enemyName = args[0].ToLower();

                if (int.TryParse(args[0], out enemyIndex))
                {
                    if (enemyIndex < DanceTools.spawnableEnemies.Count)
                    {
                        enemyName = DanceTools.spawnableEnemies[enemyIndex].name.ToLower();
                    }
                    else
                    {
                        DTConsole.Instance.PushTextToOutput($"Enemy {enemyIndex} doesn't exist in current list.\nSometimes you need to load a certain map to load an enemy reference.", DanceTools.consoleErrorColor);
                        return;
                    }
                }

                DanceTools.SpawnableEnemy enemyToSpawn;
                int amount = 1;
                string outsideInsideText = "";
                Vector3 spawnPos = Vector3.zero;
                int specialSpawnCase = -1; //-1 = default spawn

                //amount check
                if (args.Length > 1)
                {
                    amount = DanceTools.CheckInt(args[1]);
                    if (amount == -1) return;
                    if (amount <= 0)
                    {
                        DTConsole.Instance.PushTextToOutput($"Amount cannot be 0 or less than 0", DanceTools.consoleErrorColor);
                        return;
                    }
                }
                //check if enemy exists in the master list
                if (!DanceTools.spawnableEnemies.Any((x) => x.name.ToLower().Contains(enemyName)))
                {
                    DTConsole.Instance.PushTextToOutput($"Enemy {enemyName} doesn't exist in current list.\nSometimes you need to load a certain map to load an enemy reference.", DanceTools.consoleErrorColor);
                    return;
                }

                //get enemy to spawn
                enemyToSpawn = DanceTools.spawnableEnemies.Find((x) => x.name.ToLower().Contains(enemyName));

                //check if inside or outside text
                outsideInsideText = enemyToSpawn.isOutside ? "outside on a random node" : "inside a random vent";

                //specialSpawnCase
                //-1 = default
                //0 = onme
                //1 = inside
                //2 = outside

                //bit stupid ngl
                //if special case, then choose which case to spawn 
                if (args.Length > 2)
                {
                    switch (args[2])
                    {
                        case "onme":
                            specialSpawnCase = 0; 
                            //if player alive, spawn it on top of the player
                            spawnPos = GameNetworkManager.Instance.localPlayerController.transform.position;
                            outsideInsideText = "on top of you..";

                            //if is dead, then choose the spectated player instead
                            if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                            {
                                spawnPos = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.transform.position;
                                outsideInsideText = $"on {GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.playerUsername}";
                            }
                            break;//break

                        case "inside":
                            specialSpawnCase = 1;
                            outsideInsideText = "inside a random vent";
                            break;//break

                        case "outside":
                            specialSpawnCase = 2;
                            outsideInsideText = "outside on a random node";
                            break;//break

                        default:
                            specialSpawnCase = -1;
                            break;
                    }
                }

                DTConsole.Instance.PushTextToOutput(amount.ToString());

                //check where to spawn enemies and spawn them
                switch (specialSpawnCase)
                {
                    case -1: //default spawn
                        //inside or outside spawn depending on where enemy is meant to be
                        if (enemyToSpawn.isOutside)
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                OutsideSpawner(enemyToSpawn);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < amount; i++)
                            {
                                InsideSpawner(enemyToSpawn);
                            }
                        }
                        break;

                    case 0: //onme case. spawn it on top of the player
                        NetworkStuff.SendEnemyMessage(new NetworkStuff.SerializableGenericSpawnData(DanceTools.spawnableEnemies.IndexOf(enemyToSpawn), spawnPos, (uint)amount));
                        break;

                    case 1: //inside spawn
                        for (int i = 0; i < amount; i++)
                        {
                            InsideSpawner(enemyToSpawn);
                        }
                        break;

                    case 2: //outside spawn
                        for (int i = 0; i < amount; i++)
                        {
                            OutsideSpawner(enemyToSpawn);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                DTConsole.Instance.PushTextToOutput("Can't spawn enemies when not landed.", DanceTools.consoleErrorColor);
                DanceTools.mls.LogError($"error: {e.Message}");
            }
        }

        private void InsideSpawner(DanceTools.SpawnableEnemy enemyToSpawn)
        {
            int randomIndex = UnityEngine.Random.Range(0, DanceTools.currentRound.allEnemyVents.Length);
            NetworkStuff.SendEnemyMessage(new NetworkStuff.SerializableGenericSpawnData(DanceTools.spawnableEnemies.IndexOf(enemyToSpawn), DanceTools.currentRound.allEnemyVents[randomIndex].floorNode.position, 1));
        }
        private void OutsideSpawner(DanceTools.SpawnableEnemy enemyToSpawn)
        {
            int randomIndex = UnityEngine.Random.Range(0, DanceTools.currentRound.outsideAINodes.Length);
            NetworkStuff.SendEnemyMessage(new NetworkStuff.SerializableGenericSpawnData(DanceTools.spawnableEnemies.IndexOf(enemyToSpawn), DanceTools.currentRound.outsideAINodes[randomIndex].transform.position, 1));
        }
    }
}