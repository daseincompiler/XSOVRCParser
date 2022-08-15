using System.Text.RegularExpressions;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal static class VRCEventHandler
{
    private static RoomInformation _roomInformation;

    private static string? _localDisplayName;

    private static bool _shouldLog;

    private static int _lastPlayerCount;

    private static int _pseudoHardMax;

    public static void AssignEvents()
    {
        VRChatEvents.UserAuthenticated += s =>
        {
            var match = Regex.Match(s, @"User Authenticated: (.+)");

            XSOLog.PrintLog(match);

            _localDisplayName = match.Groups[1].Value;
        };

        VRChatEvents.ServerVersion += s => XSOLog.PrintLog(Regex.Match(s, @"server version: (.+)\S"));

        VRChatEvents.ConnectedToMaster += s =>
        {
            var match = Regex.Match(s, @"Switching to network region (.+)").Groups[1];

            var region = match.Value;

            _roomInformation.Region = region;
        };

        VRChatEvents.EnteredRoom += s =>
        {
            var match = Regex.Match(s, @"Entering Room: (.+)");

            _roomInformation.RoomName = match.Groups[1].Value;
        };

        VRChatEvents.SuccessJoinedRoom += () =>
        {
            XSOLog.PrintLog($"Successfully joined: {_roomInformation.RoomName}, Region: {_roomInformation.Region}",
                ConsoleColor.Cyan);
            _shouldLog = true;
        };

        VRChatEvents.PlayerJoined += s =>
        {
            //might be a useless check, but just in case
            if (!_shouldLog) return;

            var match = Regex.Match(s, @"OnPlayerJoined (.+)");

            if (string.IsNullOrEmpty(match.Value)) return;

            _lastPlayerCount++;

            var displayName = match.Groups[1].Value;

            if (displayName == _localDisplayName) return;

            _pseudoHardMax = 2 * _lastPlayerCount;

            XSOLog.PrintLog($"Player [{_lastPlayerCount}/{_pseudoHardMax}] joined: {displayName}", ConsoleColor.White);
        };

        VRChatEvents.PlayerLeft += s =>
        {
            if (!_shouldLog) return;

            var match = Regex.Match(s, @"OnPlayerLeft (.+)");

            if (string.IsNullOrEmpty(match.Value)) return;

            _lastPlayerCount--;

            var displayName = match.Groups[1].Value;

            if (displayName == _localDisplayName) return;

            _pseudoHardMax = 2 * _lastPlayerCount;

            XSOLog.PrintLog($"Player [{_lastPlayerCount}/{_pseudoHardMax}] left: {displayName}", ConsoleColor.White);
        };

        VRChatEvents.LeftRoom += s =>
        {
            var match = Regex.Match(s, @"Behaviour] (.+)").Groups[1];
            XSOLog.PrintLog(match);
            _shouldLog = false;
            _lastPlayerCount = 0;
        };

        VRChatEvents.ApplicationQuit += s =>
        {
            XSOLog.PrintLog(Regex.Match(s, @"VRCApplication: (.+)\S"));
            File.AppendAllText($"XSOVRCParser_{DateTime.Now.ToString("ddMM_hhmm")}.txt", XSOLog.Log.ToString());
            XSOLog.Log.Clear();
            // Environment.Exit(0);
        };
    }

    public static void GetEvents(string input)
    {
        switch (input)
        {
            case { } behaviour when behaviour.Contains("[Behaviour]"):

                if (behaviour.Contains("User Authenticated:")) VRChatEvents.OnUserAuthenticated(input);

                if (behaviour.Contains("Using network server version:")) VRChatEvents.OnServerVersion(input);

                if (behaviour.Contains("Switching to network")) VRChatEvents.OnConnectedToMaster(input);

                if (behaviour.Contains("Entering Room:")) VRChatEvents.OnEnteredRoom(input);

                if (behaviour.Contains("Successfully joined room")) VRChatEvents.OnSuccessJoinedRoom();

                if (behaviour.Contains("OnPlayerJoined")) VRChatEvents.OnPlayerJoined(input);

                if (behaviour.Contains("OnPlayerLeft")) VRChatEvents.OnPlayerLeft(input);

                if (behaviour.Contains("OnLeftRoom")) VRChatEvents.OnLeftRoom(input);
                break;

            case "VRCApplication: OnApplicationQuit a":
                VRChatEvents.OnApplicationQuit(input);
                break;
        }
    }

    private struct RoomInformation
    {
        public string RoomName;
        public string Region;
    }
}