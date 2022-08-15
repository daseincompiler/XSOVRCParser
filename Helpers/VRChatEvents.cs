namespace XSOVRCParser.Helpers;

internal class VRChatEvents
{
    //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving
    public static event Action<string> UserAuthenticated = null!;

    public static event Action<string> ServerVersion = null!;

    public static event Action<string> SwitchingToNetwork = null!;

    public static event Action<string> EnteringRoom = null!;

    public static event Action<string> JoiningRoom = null!; 

    public static event Action SuccessJoinedRoom = null!;

    public static event Action<string> PlayerJoined = null!;

    public static event Action<string> PlayerLeft = null!;

    public static event Action LeftRoom = null!;

    public static event Action<string> ApplicationQuit = null!;

    public void OnUserAuthenticated(string input) => UserAuthenticated(input);
    public void OnServerVersion(string input) => ServerVersion(input);
    public void OnSwitchingToNetwork(string input) => SwitchingToNetwork(input);
    public void OnEnteringRoom(string input) => EnteringRoom(input);
    public void OnJoiningRoom(string input) => JoiningRoom(input);
    public void OnSuccessJoinedRoom() => SuccessJoinedRoom();
    public void OnPlayerJoined(string input) => PlayerJoined(input);
    public void OnPlayerLeft(string input) => PlayerLeft(input);
    public void OnLeftRoom() => LeftRoom();
    public void OnApplicationQuit(string input) => ApplicationQuit(input);
}