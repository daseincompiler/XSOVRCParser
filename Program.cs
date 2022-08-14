using XSOVRCParser;

var pIn = new ProgramInitializer();

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
            VRCEventHandler.GetEvents(sanitizedString);
            var inputType = XSOLog.GetInputType(sanitizedString, out var outputToPrint);
            XSOLog.ConsoleWrite(inputType, outputToPrint);
        }
        else
            wh.WaitOne(1000);
    }
}