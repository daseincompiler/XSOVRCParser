using SharpOSC;
using XSOVRCParser.Helpers;

namespace XSOVRCParser;

internal abstract class OSCMessage
{
    private static readonly UDPSender Sender = new(OSCHelper.Ip, OSCHelper.Port);

    public static async void SendChatBox(string message)
    {
        try
        {
            if (message.Length > OSCHelper.MaxLength) return;

            var oscTyping = new OscMessage("/chatbox/typing", true);
            Sender.Send(oscTyping);

            var oscMessage = new OscMessage("/chatbox/input", message, true, true);
            Sender.Send(oscMessage);

            await Task.Delay(10000);

            ClearMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static void ClearMessage()
    {
        Sender.Send(new OscMessage("/chatbox/input", string.Empty, true));
        Sender.Send(new OscMessage("/chatbox/typing", false));
    }
}