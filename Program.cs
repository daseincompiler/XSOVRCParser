using XSOVRCParser;
using XSOVRCParser.Helpers;

var pIn = new Initializer();

VRCEventHandler.AssignEvents();

var wh = new AutoResetEvent(false);
var fsw = new FileSystemWatcher(pIn.VRCDirectory.FullName);
fsw.Filter = pIn.LastWrittenFile.Name;
fsw.EnableRaisingEvents = true;
fsw.Changed += (_, _) => wh.Set();

var fs = new FileStream(pIn.LastWrittenFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
using var sr = new StreamReader(fs);
{
    while (true)
    {
        var s = sr.ReadLine();
        if (s != null)
        {
            var sanitizedString = s.Trim();
            var inputType = XSOLog.GetInputType(sanitizedString, out var outputToPrint);
            XSOLog.ConsoleWrite(inputType, outputToPrint);
            VRCEvents.GetEvents(sanitizedString);
        }
        else
            wh.WaitOne(1000);
    }
}