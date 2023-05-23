using SharpOSC;
using XSOVRCParser.Helpers;

namespace XSOVRCParser
{
    internal class OSCMessage
    {
        private static UDPSender sender  = new UDPSender(OSCHelper.IP, OSCHelper.Port);
        public static void sendChatBox(string message)
        {
            try {
                var oscTyping = new OscMessage("/chatbox/typing", true);
                sender.Send(oscTyping);

                var oscMessage = new OscMessage("/chatbox/input", message, true);
                sender.Send(oscMessage);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
