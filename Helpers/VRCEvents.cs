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

    public static event Action<string> OnPlayerLeft = null!;

    public static event Action OnLeftRoom = null!;

    public static event Action<string> OnApplicationQuit = null!;

    public static void GetEvents(string input)
    {
        switch (input)
        {
            case { } behaviour when behaviour.Contains("[Behaviour]"):

                if (behaviour.Contains("User Authenticated:")) OnUserAuthenticated(input);

                if (behaviour.Contains("Using network server version:")) OnServerVersion(input);

                if (behaviour.Contains("Switching to network")) OnSwitchingToNetwork(input);

                if (behaviour.Contains("Entering Room:")) OnEnteringRoom(input);

                if (behaviour.Contains("Joining wrld_")) OnJoiningRoom(input);

                if (behaviour.Contains("Successfully joined room")) OnSuccessJoinedRoom();

                if (behaviour.Contains("OnPlayerJoined")) OnPlayerJoined(input);

                if (behaviour.Contains("OnPlayerLeft")) OnPlayerLeft(input);

                if (behaviour.Contains("OnLeftRoom")) OnLeftRoom();
                break;

            case { } applicationQuit when applicationQuit.Contains("VRCApplication: OnApplicationQuit at"):
                OnApplicationQuit(input);
                break;
        }
    }
}