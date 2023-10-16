using System.Text.RegularExpressions;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal static class VRCEventHandler
{
    private static RoomInformation _roomInformation;
    private static readonly List<string> Players = new();

    private static string? _localDisplayName;
    private static bool _shouldLog;
    private static int _lastPlayerCount;
    private static bool _shouldLogSwitch;
    private static string? _previousSwitch;
    private static int PseudoHardMax => _lastPlayerCount >= 40 ? 80 : 2 * _lastPlayerCount;

    public static void AssignEvents()
    {
        VRCEvents.OnUserAuthenticated += HandleUserAuthenticated;

        VRCEvents.OnServerVersion += s => XSOLog.PrintLog(Regex.Match(s, @"server version: (.+)\S"));

        VRCEvents.OnSwitchingToNetwork += s =>
            _roomInformation.Region = Regex.Match(s, @"Switching to network region (.+)").Groups[1].Value.Trim();

        VRCEvents.OnEnteringRoom +=
            s => _roomInformation.RoomName = Regex.Match(s, @"Entering Room: (.+)").Groups[1].Value.Trim();

        VRCEvents.OnJoiningRoom += HandleOnJoiningRoom;

        VRCEvents.OnSuccessJoinedRoom += HandleOnSuccessJoinedRoom;

        VRCEvents.OnPlayerJoined += HandlePlayerJoined;

        VRCEvents.OnPlayerSwitchedAvatar += HandlePlayerSwitchedAvatar;

        VRCEvents.OnPlayerLeft += HandlePlayerLeft;

        VRCEvents.OnLeftRoom += HandleRoomExit;

        VRCEvents.OnDisconnected += HandleDisconnection;

        VRCEvents.OnApplicationQuit += HandleApplicationQuit;
    }

    private static void HandleUserAuthenticated(string s)
    {
        var match = Regex.Match(s, @"User Authenticated: (\S+)");
        if (!match.Success)
        {
            XSOLog.PrintLog("No user Authenticated found!");
            return;
        }

        var displayName = match.Groups[1].Value;
        XSOLog.PrintLog(match);
        _localDisplayName = displayName;
    }


    private static void HandleOnJoiningRoom(string s)
    {
        var instanceId = Regex.Match(s, @"Joining (.+)").Groups[1].Value;

        _roomInformation.InstanceId = instanceId;

        var accessType = VRCAccessType.GetAccessType(instanceId);

        _roomInformation.AccessType = accessType.TranslateAccessType();
    }

    private static void HandleOnSuccessJoinedRoom()
    {
        XSOLog.PrintLog($"Joined {_roomInformation.Region.ToUpper()} '{_roomInformation.AccessType}' instance '{_roomInformation.RoomName}' -> {_roomInformation.InstanceId}");

        _shouldLog = true;
        _shouldLogSwitch = false;
    }

    private static void HandlePlayerJoined(string s)
    {
        if (!_shouldLog) return;

        var match = Regex.Match(s, @"OnPlayerJoined (.+)");
        if (string.IsNullOrEmpty(match.Value)) return;

        var displayName = match.Groups[1].Value;
        if (displayName == _localDisplayName) return;

        Players.Add(displayName);

        _shouldLogSwitch = true;
        _lastPlayerCount++;

        XSOLog.PrintLog($"[{_lastPlayerCount}/{PseudoHardMax}] {displayName} has joined", ConsoleColor.White);
    }

    private static void HandlePlayerSwitchedAvatar(string s)
    {
        if (!_shouldLog || !_shouldLogSwitch) return;

        var match = Regex.Match(s, @"Switching (.+)");

        if (string.IsNullOrEmpty(match.Value)) return;

        if (_localDisplayName == null) return;

        if (match.Value.Contains(_localDisplayName)) return;

        if (match.Value == _previousSwitch) return;

        _previousSwitch = match.Value;

        OSCMessage.SendChatBox(match.Value);

        XSOLog.PrintLog($"{match.Value}");
    }

    private static void HandlePlayerLeft(string s)
    {
        if (!_shouldLog || _lastPlayerCount == 0) return;

        var match = Regex.Match(s, @"OnPlayerLeft (.+)");
        if (!match.Success) return;

        var displayName = match.Groups[1].Value;
        if (displayName == _localDisplayName) return;

        Players.Remove(displayName);
        _lastPlayerCount--;

        XSOLog.PrintLog($"[{_lastPlayerCount}/{PseudoHardMax}] {displayName} has left", ConsoleColor.White);
    }

    private static void HandleRoomExit()
    {
        XSOLog.PrintLog($"Left {_roomInformation.Region.ToUpper()} '{_roomInformation.AccessType}' instance '{_roomInformation.RoomName}' -> {_roomInformation.InstanceId}");
        ResetRoomState();
    }

    private static void HandleDisconnection(string s)
    {
        if (!_shouldLog) return;

        var match = Regex.Match(s, @"OnDisconnected\((.+)\)");
        if (!match.Success) return;

        var disconnectReason = match.Groups[1].Value;
        XSOLog.PrintLog(disconnectReason, ConsoleColor.Red);
    }

    private static void HandleApplicationQuit(string s)
    {
        var match = Regex.Match(s, @"VRCApplication: (.+)\S");
        if (match.Success)
        {
            var applicationMessage = match.Groups[1].Value;
            XSOLog.PrintLog(applicationMessage);
        }

        SaveLogToFile();
        XSOLog.Log.Clear();
    }

    private static void SaveLogToFile()
    {
        var fileName = $"XSOVRCParser_{DateTime.Now.ToString("ddMM_hhmm")}.txt";
        File.AppendAllText(fileName, XSOLog.Log.ToString());
    }

    private static void ResetRoomState()
    {
        _shouldLog = false;
        _lastPlayerCount = 0;
        Players.Clear();
    }

    private struct RoomInformation
    {
        public string RoomName { get; set; }
        public string InstanceId { get; set; }
        public string AccessType { get; set; }
        public string Region { get; set; }
    }
}