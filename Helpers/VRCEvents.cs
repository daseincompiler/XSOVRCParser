namespace XSOVRCParser.Helpers;

//made the class public in case someone would like to subscribe to these events
public static class VRCEvents
{
    //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-forgiving
    public static event Action<string> OnUserAuthenticated = null!;

    public static event Action<string> OnServerVersion = null!;

    public static event Action<string> OnSwitchingToNetwork = null!;

    public static event Action<string> OnEnteringRoom = null!;

    public static event Action<string> OnJoiningRoom = null!;

    public static event Action OnSuccessJoinedRoom = null!;

    public static event Action<string> OnPlayerJoined = null!;

    public static event Action<string> OnPlayerSwitchedAvatar = null!;

    public static event Action<string> OnKeywordsExceeded = null!;

    public static event Action<string> OnPlayerLeft = null!;

    public static event Action OnLeftRoom = null!;

    public static event Action<string> OnDisconnected = null!;

    public static event Action<string> OnApplicationQuit = null!;

    public static void GetEvents(string input)
    {
        switch (input)
        {
            case not null when input.Contains("[Behaviour]"):

                if (input.Contains("User Authenticated:")) OnUserAuthenticated(input);

                if (input.Contains("Using network server version:")) OnServerVersion(input);

                if (input.Contains("Switching to network")) OnSwitchingToNetwork(input);

                if (input.Contains("Entering Room:")) OnEnteringRoom(input);

                if (input.Contains("Joining wrld_")) OnJoiningRoom(input);

                if (input.Contains("Successfully joined room")) OnSuccessJoinedRoom();

                if (input.Contains("OnPlayerJoined")) OnPlayerJoined(input);

                if (input.Contains("to avatar")) OnPlayerSwitchedAvatar(input);

                if (input.Contains("shader global keywords exceeded")) OnKeywordsExceeded(input);

                if (input.Contains("OnPlayerLeft")) OnPlayerLeft(input);

                if (input.Contains("OnLeftRoom")) OnLeftRoom();

                if (input.Contains("OnDisconnected")) OnDisconnected(input);
                break;

            case not null when input.Contains("VRCApplication: OnApplicationQuit at"):
                OnApplicationQuit(input);
                break;
        }
    }
}