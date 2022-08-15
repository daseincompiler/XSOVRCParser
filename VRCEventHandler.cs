using System.Text.RegularExpressions;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal static class VRCEventHandler
{
    private static readonly VRCEvents VRCEvents = new();

    private static RoomInformation _roomInformation;

    private static string? _localDisplayName;

    private static bool _shouldLog;

    private static int _lastPlayerCount;

    private static int _pseudoHardMax;

    public static void AssignEvents()
    {
        VRCEvents.OnUserAuthenticated += s =>
        {
            var match = Regex.Match(s, @"User Authenticated: (.+)");

            var displayName = match.Groups[1].Value;

            XSOLog.PrintLog(match);

            _localDisplayName = displayName;
        };

        VRCEvents.OnServerVersion += s => XSOLog.PrintLog(Regex.Match(s, @"server version: (.+)\S"));

        VRCEvents.OnSwitchingToNetwork += s =>
        {
            var match = Regex.Match(s, @"Switching to network region (.+)").Groups[1];

            var region = match.Value;

            _roomInformation.Region = region;
        };

        VRCEvents.OnEnteringRoom += s =>
        {
            var match = Regex.Match(s, @"Entering Room: (.+)");

            _roomInformation.RoomName = match.Groups[1].Value;
        };

        VRCEvents.OnJoiningRoom += s =>
        {
            var instanceId = Regex.Match(s, @"Joining (.+)").Groups[1].Value;

            _roomInformation.InstanceId = instanceId;

            var accessType = VRCAccessType.GetAccessType(instanceId);

            _roomInformation.AccessType = accessType.TranslateAccessType();
        }; 

        VRCEvents.OnSuccessJoinedRoom += () =>
        {
            XSOLog.PrintLog($"Joined Instance: {_roomInformation.RoomName} -> {_roomInformation.InstanceId}",
                ConsoleColor.Cyan);
            _shouldLog = true;
        };

        VRCEvents.OnPlayerJoined += s =>
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

        VRCEvents.OnPlayerLeft += s =>
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

        VRCEvents.OnLeftRoom += () =>
        {
            XSOLog.PrintLog($"Left Instance: {_roomInformation.RoomName} -> {_roomInformation.InstanceId}",
                ConsoleColor.Cyan);
            _shouldLog = false;
            _lastPlayerCount = 0;
        };

        VRCEvents.OnApplicationQuit += s =>
        {
            XSOLog.PrintLog(Regex.Match(s, @"VRCApplication: (.+)\S"));
            File.AppendAllText($"XSOVRCParser_{DateTime.Now.ToString("ddMM_hhmm")}.txt", XSOLog.Log.ToString());
            XSOLog.Log.Clear();
            // Environment.Exit(0);
        };
    }

    private struct RoomInformation
    {
        public string RoomName;
        public string InstanceId;
        public string AccessType;
        public string Region;
    }
}