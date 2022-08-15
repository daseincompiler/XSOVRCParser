using System.Text.RegularExpressions;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal static class VRCEventHandler
{
    private static readonly VRChatEvents VRCEvents = new();

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

            var displayName = match.Groups[1].Value;

            XSOLog.PrintLog(match);

            _localDisplayName = displayName;
        };

        VRChatEvents.ServerVersion += s => XSOLog.PrintLog(Regex.Match(s, @"server version: (.+)\S"));

        VRChatEvents.SwitchingToNetwork += s =>
        {
            var match = Regex.Match(s, @"Switching to network region (.+)").Groups[1];

            var region = match.Value;

            _roomInformation.Region = region;
        };

        VRChatEvents.EnteringRoom += s =>
        {
            var match = Regex.Match(s, @"Entering Room: (.+)");

            _roomInformation.RoomName = match.Groups[1].Value;
        };

        VRChatEvents.JoiningRoom += s =>
        {
            var instanceId = Regex.Match(s, @"Joining (.+)").Groups[1].Value;

            _roomInformation.InstanceId = instanceId;

            var accessType = VRCAccessType.GetAccessType(instanceId);

            _roomInformation.AccessType = accessType.TranslateAccessType();
        }; 

        VRChatEvents.SuccessJoinedRoom += () =>
        {
            XSOLog.PrintLog($"Joined Instance: {_roomInformation.RoomName} -> {_roomInformation.InstanceId}",
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

            if (_lastPlayerCount >= 40) _pseudoHardMax = 80;
            else _pseudoHardMax = 2 * _lastPlayerCount;

            XSOLog.PrintLog($"[{_lastPlayerCount}/{_pseudoHardMax}] {displayName} has joined", ConsoleColor.White);
        };

        VRChatEvents.PlayerLeft += s =>
        {
            if (!_shouldLog) return;

            var match = Regex.Match(s, @"OnPlayerLeft (.+)");

            if (string.IsNullOrEmpty(match.Value)) return;

            if (_lastPlayerCount == 0) return;

            _lastPlayerCount--;

            var displayName = match.Groups[1].Value;

            if (displayName == _localDisplayName) return;

            if (_lastPlayerCount >= 40) _pseudoHardMax = 80;
            else _pseudoHardMax = 2 * _lastPlayerCount;

            XSOLog.PrintLog($"[{_lastPlayerCount}/{_pseudoHardMax}] {displayName} has left", ConsoleColor.White);
        };

        VRChatEvents.LeftRoom += () =>
        {
            XSOLog.PrintLog($"Left Instance: {_roomInformation.RoomName} -> {_roomInformation.InstanceId}",
                ConsoleColor.Cyan);
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

                if (behaviour.Contains("User Authenticated:")) VRCEvents.OnUserAuthenticated(input);

                if (behaviour.Contains("Using network server version:")) VRCEvents.OnServerVersion(input);

                if (behaviour.Contains("Switching to network")) VRCEvents.OnSwitchingToNetwork(input);

                if (behaviour.Contains("Entering Room:")) VRCEvents.OnEnteringRoom(input);

                if (behaviour.Contains("Joining wrld_")) VRCEvents.OnJoiningRoom(input);

                if (behaviour.Contains("Successfully joined room")) VRCEvents.OnSuccessJoinedRoom();

                if (behaviour.Contains("OnPlayerJoined")) VRCEvents.OnPlayerJoined(input);

                if (behaviour.Contains("OnPlayerLeft")) VRCEvents.OnPlayerLeft(input);

                if (behaviour.Contains("OnLeftRoom")) VRCEvents.OnLeftRoom();
                break;

            case "VRCApplication: OnApplicationQuit a":
                VRCEvents.OnApplicationQuit(input);
                break;
        }
    }

    private struct RoomInformation
    {
        public string RoomName;
        public string InstanceId;
        public string AccessType;
        public string Region;
    }
}