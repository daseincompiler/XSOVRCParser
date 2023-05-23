using XSNotifications;
using XSNotifications.Enum;

namespace XSOVRCParser;

internal static class XSONotifications
{
    public static bool ShouldNotify;

    private static readonly XSNotifier XsNotifier = new();

    public static void SendNotification(string title, string? content, string icon = "default")
    {
        if (!ShouldNotify) return;

        XsNotifier.SendNotification(new XSNotification
        {
            Title = title,
            Content = content,
            Icon = icon,
            SourceApp = nameof(XSOVRCParser),
            Volume = 0.25f,
            MessageType = XSMessageType.Notification
        });
    }
}