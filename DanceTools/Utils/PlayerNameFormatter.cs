using GameNetcodeStuff;

namespace DanceTools.Utils
{
    public static class NameUtils
    {
        public static string FormatPlayerName(PlayerControllerB player)
        {
            return $"{player.playerUsername}:{player.actualClientId}";
        }
        public static PlayerControllerB SearchForPlayer(string source)
        {
            if (uint.TryParse(source, out uint clientId))
            {
                // we've passed an id, look for a player of that id
                if (StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientId]] != null)
                {
                    return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientId]];
                }
                else
                {
                    // return null if there's no match
                    return null;
                }
            }
            else
            {
                // starts with check
                foreach (var player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (player.playerUsername.ToLower().StartsWith(source.ToLower()))
                    {
                        return player;
                    };
                }

                // if that fails, check for the search in any position
                foreach (var player in StartOfRound.Instance.allPlayerScripts)
                {
                    if (player.playerUsername.ToLower().Contains(source.ToLower()))
                    {
                        return player;
                    };
                }

                // return null if there's no match
                return null;
            }
        }
    }
}
