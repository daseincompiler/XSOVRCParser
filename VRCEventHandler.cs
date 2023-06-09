using System.Text.RegularExpressions;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal static partial class VRCEventHandler
{
    private static readonly XSOConfiguration XSOConfig = new();

    private static RoomInformation _roomInformation;

    private static string? _localDisplayName;

    private static bool _shouldLog;

    private static int _lastPlayerCount;

    private static int _pseudoHardMax;

    private static string? _lastAvatarSwitched;

    private static bool _shouldLogSwitch;

    private static string? _previousSwitch;

    private static readonly List<string> Players = new();

    public static void AssignEvents()
    {
        VRCEvents.OnUserAuthenticated += s =>
        {
            //can fail if a user has been logged out and even if you re-login again it seems to not get output log'd anymore for some reason
            var match = AuthenticatedRegex().Match(s);

            var displayName = match.Groups[1].Value;

            XSOLog.PrintLog(match);

            _localDisplayName = displayName;
        };

        VRCEvents.OnServerVersion += s => XSOLog.PrintLog(Regex.Match(s, @"server version: (.+)\S"));

        VRCEvents.OnSwitchingToNetwork += s =>
        {
            var match = RegionRegex().Match(s).Groups[1];

            var region = match.Value;

            _roomInformation.Region = region;
        };

        VRCEvents.OnEnteringRoom += s =>
        {
            var match = EnteringRegex().Match(s);

            _roomInformation.RoomName = match.Groups[1].Value.Trim();
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
            _shouldLogSwitch = false;

            var parsedInstanceId = _roomInformation.InstanceId.Split(':')[1];
            if (parsedInstanceId.Contains("~")) parsedInstanceId = parsedInstanceId.Split('~')[0];

            XSONotifications.SendNotification($"{_roomInformation.RoomName}",
                $"InstanceId: {parsedInstanceId}, AccessType: {_roomInformation.AccessType}, Region: {_roomInformation.Region}",
                XSOConfig.JoinedInstanceIconPath);
        };

        var hasSentGroup = false;

        VRCEvents.OnPlayerJoined += s =>
        {
            //might be a useless check, but just in case
            if (!_shouldLog) return;

            var match = Regex.Match(s, @"OnPlayerJoined (.+)");

            if (string.IsNullOrEmpty(match.Value)) return;

            _shouldLogSwitch = true;

            _lastPlayerCount++;

            var displayName = match.Groups[1].Value;

            if (displayName == _localDisplayName) return;

            Players.Add(displayName);

            if (_lastPlayerCount >= 40) _pseudoHardMax = 80;
            else _pseudoHardMax = 2 * _lastPlayerCount;

            XSOLog.PrintLog($"[{_lastPlayerCount}/{_pseudoHardMax}] {displayName} has joined", ConsoleColor.White);

            if (_lastPlayerCount >= 5)
            {
                if (hasSentGroup) return;
                XSONotifications.SendNotification($"[{_lastPlayerCount}/{_pseudoHardMax}] Group Join:",
                    string.Join(", ", Players), XSOConfig.PlayerJoinedInstancePath);
                hasSentGroup = true;
            }
            else
            {
                XSONotifications.SendNotification($"[{_lastPlayerCount}/{_pseudoHardMax}] {displayName} has joined",
                    null, XSOConfig.PlayerJoinedInstancePath);
            }
        };

        VRCEvents.OnPlayerSwitchedAvatar += s =>
        {
            if (!_shouldLog || !_shouldLogSwitch) return;

            var match = Regex.Match(s, @"Switching (.+)");

            if (string.IsNullOrEmpty(match.Value)) return;

            if (match.Value.Contains(_localDisplayName!)) return;

            if (match.Value == _previousSwitch) return;

            _previousSwitch = match.Value;

            _lastAvatarSwitched = Regex.Match(match.Value, @"Switching (.+) to").Groups[1].Value;

            OSCMessage.SendChatBox(match.Value);

            XSOLog.PrintLog($"{match.Value}", ConsoleColor.Cyan);
        };

        VRCEvents.OnKeywordsExceeded += s =>
        {
            if (!_shouldLog) return;

            var match = Regex.Match(s, @"shader (.+),");

            if (string.IsNullOrEmpty(match.Value)) return;

            if (string.IsNullOrEmpty(_lastAvatarSwitched)) return;

            var thankYouMessage = $"Thanks {_lastAvatarSwitched} for {match}";

            OSCMessage.SendChatBox(thankYouMessage);

            XSOLog.PrintLog($"{match.Value} for {_lastAvatarSwitched}", ConsoleColor.Yellow);
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

            Players.Remove(displayName);

            if (_lastPlayerCount >= 40) _pseudoHardMax = 80;
            else _pseudoHardMax = 2 * _lastPlayerCount;

            XSOLog.PrintLog($"[{_lastPlayerCount}/{_pseudoHardMax}] {displayName} has left", ConsoleColor.White);

            if (Players.Count >= 5)
                XSONotifications.SendNotification($"[{_lastPlayerCount}/{_pseudoHardMax}] Group Leave:",
                    string.Join(", ", Players), XSOConfig.PlayerLeftIconPath);
            else
                XSONotifications.SendNotification($"[{_lastPlayerCount}/{_pseudoHardMax}] {displayName} has left",
                    null, XSOConfig.PlayerLeftIconPath);
        };

        VRCEvents.OnLeftRoom += () =>
        {
            XSOLog.PrintLog($"Left Instance: {_roomInformation.RoomName} -> {_roomInformation.InstanceId}",
                ConsoleColor.Cyan);
            _shouldLog = false;
            _lastPlayerCount = 0;
            Players.Clear();
        };

        VRCEvents.OnDisconnected += s =>
        {
            if (!_shouldLog) return;

            var match = Regex.Match(s, @"OnDisconnected(.+)");

            if (string.IsNullOrEmpty(match.Value)) return;

            XSOLog.PrintLog($"{match.Value}", ConsoleColor.Red);
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

    [GeneratedRegex(@"User Authenticated: (.+)")]
    private static partial Regex AuthenticatedRegex();

    [GeneratedRegex(@"Switching to network region (.+)")]
    private static partial Regex RegionRegex();

    [GeneratedRegex(@"Entering Room: (.+)")]
    private static partial Regex EnteringRegex();
}