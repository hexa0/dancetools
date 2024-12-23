﻿using GameNetcodeStuff;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;

namespace DanceTools.Utils
{
    public static class NetworkStuff
    {

        public static void Awake()
        {
            cheatsToggledMessage.OnReceivedFromClient += OnCheatsToggledMessage;
            enemyMessage.OnReceivedFromClient += OnEnemyMessage;
            itemMessage.OnReceivedFromClient += OnItemMessage;
            creditsMessage.OnReceivedFromClient += OnCreditsMessage;
            lightsMessage.OnReceivedFromClient += OnLightsMessage;
            teleportMessage.OnReceivedFromClient += OnTeleportMessage;
        }

        public static PlayerControllerB CurrentClient
        {
            get
            {
                return GameNetworkManager.Instance.localPlayerController;
            }
        }

        public static bool isHost
        {
            get
            {
                return CurrentClient.actualClientId == 0;
            }
        }

        private static LethalClientMessage<bool> cheatsToggledMessage = new LethalClientMessage<bool>(identifier: "danceToolsCheatsToggled");

        public static void SendCheatsToggledMessage(bool enabled)
        {
            if (isHost)
            {
                try
                {
                    cheatsToggledMessage.SendAllClients(enabled);
                }
                catch
                {
                    DanceTools.mls.LogError("[NetworkStuff] [SendCheatsToggledMessage] failed!!!");
                }
            }
        }

        private static void OnCheatsToggledMessage(bool enabled, ulong clientId)
        {
            if (clientId == 0)
            {
                DanceTools.cheatsEnabled = enabled;

                if (enabled)
                {
                    DTConsole.Instance.PushTextToOutput("sv_cheats has been set to 1 by the host", DanceTools.consoleInfoColor);
                }
                else
                {
                    DTConsole.Instance.PushTextToOutput("sv_cheats has been set to 0 by the host", DanceTools.consoleInfoColor);
                }
            }
            else
            {
                DTConsole.Instance.PushTextToOutput($"an attempt to set sv_cheats from non-host ({clientId}) has been blocked", DanceTools.consoleInfoColor);
            }
        }

        [System.Serializable]
        public class SerializableGenericSpawnData
        {
            public int id;
            public Vector3 location;
            public uint count;

            public SerializableGenericSpawnData(int id, Vector3 location, uint count)
            {
                this.id = id;
                this.location = location;
                this.count = count;
            }
        }

        [System.Serializable]
        public class SerializableItemSpawnData : SerializableGenericSpawnData
        {
            public float weight;
            public int value;

            public SerializableItemSpawnData(int id, Vector3 location, uint count, float weight, int value) : base(id, location, count)
            {
                this.id = id;
                this.location = location;
                this.count = count;

                this.weight = weight;
                this.value = value;
            }
        }

        private static LethalClientMessage<SerializableGenericSpawnData> enemyMessage = new LethalClientMessage<SerializableGenericSpawnData>(identifier: "danceToolsEnemySpawn");

        public static void SendEnemyMessage(SerializableGenericSpawnData spawnMessage)
        {
            try
            {
                enemyMessage.SendAllClients(spawnMessage);
            }
            catch
            {
                DanceTools.mls.LogError("[NetworkStuff] [SendCheatsToggledMessage] failed!!!");
            }
        }

        private static void OnEnemyMessage(SerializableGenericSpawnData spawnMessage, ulong clientId)
        {
            if (DanceTools.cheatsEnabled)
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} has spawned {spawnMessage.count} '{DanceTools.spawnableEnemies[spawnMessage.id].name}' enemy(s) at '{spawnMessage.location}'", DanceTools.consoleInfoColor);
                if (isHost)
                {
                    for (int i = 0; i < spawnMessage.count; i++)
                    {
                        GameObject enemy = Object.Instantiate(DanceTools.spawnableEnemies[spawnMessage.id].prefab, spawnMessage.location, Quaternion.identity);
                        enemy.GetComponentInChildren<NetworkObject>().Spawn(destroyWithScene: true);
                    }
                }
            }
            else
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} was blocked from sending {spawnMessage} as cheats are disabled");
            }
        }

        private static LethalClientMessage<SerializableItemSpawnData> itemMessage = new LethalClientMessage<SerializableItemSpawnData>(identifier: "danceToolsItemSpawn");

        public static void SendItemMessage(SerializableItemSpawnData spawnMessage)
        {
            try
            {
                itemMessage.SendAllClients(spawnMessage);
            }
            catch
            {
                DanceTools.mls.LogError("[NetworkStuff] [SendCheatsToggledMessage] failed!!!");
            }
        }

        private static void OnItemMessage(SerializableItemSpawnData spawnMessage, ulong clientId)
        {
            if (DanceTools.cheatsEnabled)
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} has spawned {spawnMessage.count} '{StartOfRound.Instance.allItemsList.itemsList[spawnMessage.id].itemName}' item(s) at `{spawnMessage.location}` valued at `{spawnMessage.value}` weighing `{spawnMessage.weight}`", DanceTools.consoleInfoColor);
                if (isHost)
                {
                    for (int i = 0; i < spawnMessage.count; i++)
                    {
                        GameObject item = Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[spawnMessage.id].spawnPrefab, spawnMessage.location, Quaternion.identity);

                        ScanNodeProperties scan = item.GetComponent<ScanNodeProperties>();
                        if (scan == null)
                        {
                            scan = item.AddComponent<ScanNodeProperties>(); //attach scanning node for value
                        }

                        scan.scrapValue = spawnMessage.value;
                        scan.subText = $"Value: ${spawnMessage.value}";

                        if (spawnMessage.weight != -1f)
                        {
                            item.GetComponent<GrabbableObject>().itemProperties.weight = spawnMessage.weight;
                        }

                        item.GetComponent<GrabbableObject>().itemProperties.creditsWorth = spawnMessage.value;
                        item.GetComponent<GrabbableObject>().SetScrapValue(spawnMessage.value);
                        item.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }
            else
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} was blocked from sending {spawnMessage} as cheats are disabled");
            }
        }

        private static LethalClientMessage<int> creditsMessage = new LethalClientMessage<int>(identifier: "danceToolsSetCredits");

        public static void SendCreditsMessage(int creditsAmount)
        {
            try
            {
                creditsMessage.SendAllClients(creditsAmount);
            }
            catch
            {
                DanceTools.mls.LogError("[NetworkStuff] [SendCheatsToggledMessage] failed!!!");
            }
        }

        private static void OnCreditsMessage(int creditsAmount, ulong clientId)
        {
            if (DanceTools.cheatsEnabled)
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} has set the credits to {creditsAmount}", DanceTools.consoleInfoColor);
                if (isHost)
                {
                    Terminal terminal = Object.FindObjectOfType<Terminal>();

                    Object.FindObjectOfType<Terminal>().SyncGroupCreditsServerRpc(creditsAmount, terminal.numberOfItemsInDropship);
                }
            }
            else
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} was blocked from setting the credits to {creditsAmount} as cheats are disabled");
            }
        }

        private static LethalClientMessage<bool> lightsMessage = new LethalClientMessage<bool>(identifier: "danceToolsLightsMessage");

        public static void SendLightsMessage(bool lightsEnabled)
        {
            try
            {
                lightsMessage.SendAllClients(lightsEnabled);
            }
            catch
            {
                DanceTools.mls.LogError("[NetworkStuff] [SendCheatsToggledMessage] failed!!!");
            }
        }

        private static void OnLightsMessage(bool lightsEnabled, ulong clientId)
        {
            if (DanceTools.cheatsEnabled)
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} has set the indoor light state to {lightsEnabled}", DanceTools.consoleInfoColor);
                if (isHost)
                {
                    DanceTools.currentRound.SwitchPower(lightsEnabled);
                }
            }
            else
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} was blocked from setting the indoor light state to {lightsEnabled} as cheats are disabled");
            }
        }

        [System.Serializable]
        public class SerializableTeleportData
        {
            public Vector3 targetPosition;
            public Quaternion targetRotation;
            public ulong[] targets;

            public SerializableTeleportData(Vector3 targetPosition, Quaternion targetRotation, ulong[] targets)
            {
                this.targetPosition = targetPosition;
                this.targetRotation = targetRotation;
                this.targets = targets;
            }
        }

        private static LethalClientMessage<SerializableTeleportData> teleportMessage = new LethalClientMessage<SerializableTeleportData>(identifier: "danceToolsTeleport");

        public static void SendTeleportMessage(Vector3 targetPosition, Quaternion targetRotation, ulong[] targets)
        {
            try
            {
                teleportMessage.SendAllClients(new SerializableTeleportData(targetPosition, targetRotation, targets));
            }
            catch
            {
                DanceTools.mls.LogError("[NetworkStuff] [SendTeleportMessage] failed!!!");
            }
        }

        private static void OnTeleportMessage(SerializableTeleportData teleportMessage, ulong clientId)
        {
            if (DanceTools.cheatsEnabled)
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} has teleported {teleportMessage.targets.Length} player(s) to {teleportMessage.targetPosition} facing {teleportMessage.targetRotation}", DanceTools.consoleInfoColor);
                foreach (var item in teleportMessage.targets)
                {
                    if (item == CurrentClient.actualClientId)
                    {
                        CurrentClient.transform.position = teleportMessage.targetPosition;
                        CurrentClient.transform.rotation = teleportMessage.targetRotation;
                        break;
                    }
                }
            }
            else
            {
                DTConsole.Instance.PushTextToOutput($"{NameUtils.FormatPlayerName(clientId.GetPlayerController())} was blocked from teleporting {teleportMessage.targets.Length} player(s) as cheats are disabled");
            }
        }
    }
}