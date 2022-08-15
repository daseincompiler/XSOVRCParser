namespace XSOVRCParser.Helpers;

internal static class VRCAccessType
{
    public enum InstanceAccessType
    {
        InviteOnly = 0,
        InvitePlus = 1,
        FriendsOnly = 2,
        FriendsPlus = 3,
        Public = 4
    }

    public static InstanceAccessType GetAccessType(string instanceId)
    {
        if (instanceId.Contains("hidden"))
            return InstanceAccessType.FriendsPlus;

        if (instanceId.Contains("friends"))
            return InstanceAccessType.FriendsOnly;

        if (instanceId.Contains("private") && instanceId.Contains("canRequestInvite"))
            return InstanceAccessType.InvitePlus;

        return instanceId.Contains("private") ? InstanceAccessType.InviteOnly : InstanceAccessType.Public;
    }

    public static string TranslateAccessType(this InstanceAccessType accessType)
    {
        return accessType switch
        {
            InstanceAccessType.InviteOnly => "Invite",
            InstanceAccessType.InvitePlus => "Invite+",
            InstanceAccessType.FriendsOnly => "Friends",
            InstanceAccessType.FriendsPlus => "Friends+",
            InstanceAccessType.Public => "Public",
            _ => throw new ArgumentOutOfRangeException(nameof(accessType), accessType, null)
        };
    }
}