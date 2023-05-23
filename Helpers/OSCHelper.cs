
namespace XSOVRCParser.Helpers
{
    internal class OSCHelper
    {
        // Port to send the messages to.
        public const string IP = "127.0.0.1";
        // Port to send the messages to. VRChats standard port is 9000
        public const int Port = 9000;
        // VRChats Maximum Amount of Characters.
        public const int MaxLength = 144;
        // VRChats rate limit
        public const int RateLimit = 1300;
    }
}
