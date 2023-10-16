#pragma warning disable CS8618
namespace XSOVRCParser.Helpers;

public static class VRCEvents
{
    public static event Action<string> OnUserAuthenticated;

    public static event Action<string> OnServerVersion;

    public static event Action<string> OnSwitchingToNetwork;

    public static event Action<string> OnEnteringRoom;

    public static event Action<string> OnJoiningRoom;

    public static event Action OnSuccessJoinedRoom;

    public static event Action<string> OnPlayerJoined;

    public static event Action<string> OnPlayerSwitchedAvatar;

    public static event Action<string> OnPlayerLeft;

    public static event Action OnLeftRoom;

    public static event Action<string> OnDisconnected;

    public static event Action<string> OnApplicationQuit;

    public static void GetEvents(string? input)
    {
        if (input == null) return;

        if (input.Contains("[Behaviour]"))
            HandleBehaviourEvents(input);
        else if (input.Contains("User Authenticated")) InvokeEvent(OnUserAuthenticated, input);
        else if (input.Contains("VRCApplication: OnApplicationQuit at")) InvokeEvent(OnApplicationQuit, input);
    }

    private static void HandleBehaviourEvents(string input)
    {
        if (input.Contains("Using network server version")) InvokeEvent(OnServerVersion, input);

        if (input.Contains("Switching to network")) InvokeEvent(OnSwitchingToNetwork, input);

        if (input.Contains("Entering Room:")) InvokeEvent(OnEnteringRoom, input);

        if (input.Contains("Joining wrld_")) InvokeEvent(OnJoiningRoom, input);

        if (input.Contains("Successfully joined room")) InvokeEvent(OnSuccessJoinedRoom);

        if (input.Contains("OnPlayerJoined")) InvokeEvent(OnPlayerJoined, input);

        if (input.Contains("to avatar")) InvokeEvent(OnPlayerSwitchedAvatar, input);

        if (input.Contains("OnPlayerLeft")) InvokeEvent(OnPlayerLeft, input);

        if (input.Contains("OnLeftRoom")) InvokeEvent(OnLeftRoom);

        if (input.Contains("OnDisconnected")) InvokeEvent(OnDisconnected, input);
    }

    private static void InvokeEvent(Action<string>? actionEvent, string input)
    {
        actionEvent?.Invoke(input);
    }

    private static void InvokeEvent(Action? actionEvent)
    {
        actionEvent?.Invoke();
    }
}