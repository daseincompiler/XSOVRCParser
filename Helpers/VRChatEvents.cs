namespace XSOVRCParser.Helpers;

internal static class VRChatEvents
{
    //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving
    public static event Action<string> UserAuthenticated = null!;
    public static event Action<string> ServerVersion = null!;
    public static event Action<string> ConnectedToMaster = null!;
    public static event Action<string> EnteredRoom = null!;
    public static event Action SuccessJoinedRoom = null!;
    public static event Action<string> PlayerJoined = null!;
    public static event Action<string> PlayerLeft = null!;
    public static event Action<string> LeftRoom = null!;
    public static event Action<string> ApplicationQuit = null!;

    public static void OnUserAuthenticated(string input) => UserAuthenticated(input);
    public static void OnServerVersion(string input) => ServerVersion(input);
    public static void OnConnectedToMaster(string input) => ConnectedToMaster(input);
    public static void OnEnteredRoom(string input) => EnteredRoom(input);
    public static void OnSuccessJoinedRoom() => SuccessJoinedRoom();
    public static void OnPlayerJoined(string input) => PlayerJoined(input);
    public static void OnPlayerLeft(string input) => PlayerLeft(input);
    public static void OnLeftRoom(string input) => LeftRoom(input);
    public static void OnApplicationQuit(string input) => ApplicationQuit(input);

}